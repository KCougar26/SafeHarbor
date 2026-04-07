using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SafeHarbor.Authorization;
using SafeHarbor.Infrastructure;
using SafeHarbor.Services;
using SafeHarbor.Services.DonorImpact;

var builder = WebApplication.CreateBuilder(args);

// NOTE: JSON console formatting keeps logs structured for Azure Log Analytics and App Insights queries.
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// NOTE: Policy names are centralized so controllers can enforce consistent role semantics.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.AdminOnly, policy => policy.RequireRole("Admin"));

    // DonorOnly restricts donor dashboard endpoints to users with the "Donor" role.
    // Currently the donor controller uses [AllowAnonymous] for local dev; this policy
    // will be enforced once Entra ID authentication is fully wired.
    options.AddPolicy(PolicyNames.DonorOnly, policy => policy.RequireRole("Donor"));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// TODO: Register production database services when persistence integration is implemented.
builder.Services.AddSingleton<InMemoryDataStore>();
builder.Services.AddSingleton<IAuditLogger, AuditLogger>();
builder.Services.AddSingleton<IDataRetentionRedactionService, DataRetentionRedactionService>();

// Donor impact calculator — used by DonorDashboardController to compute "girls helped" metric.
// TO SWAP IN AN ML MODEL: replace RuleBasedImpactCalculator with your MlImpactCalculator class here.
// The controller and frontend are unaffected by this change.
builder.Services.AddSingleton<IDonorImpactCalculator, RuleBasedImpactCalculator>();

// NOTE: Live/ready probes support platform health checks and safer blue/green swaps.
builder.Services.AddHealthChecks();

// NOTE: Baseline telemetry is enabled with OTLP export so staging dashboards can read traces and metrics.
var telemetryServiceName = builder.Configuration["Telemetry:ServiceName"] ?? "safeharbor-api";
var otlpEndpoint = builder.Configuration["Telemetry:OtlpEndpoint"];

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(telemetryServiceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        }
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        }
    });

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed the in-memory store with test donors, a campaign, and contribution history.
// This allows the donor dashboard to render immediately without a real database.
// See Infrastructure/DonorDashboardSeeder.cs for test credentials and amounts.
// TODO: Remove once a real database with migration seeds is in place.
DonorDashboardSeeder.Seed(app.Services.GetRequiredService<InMemoryDataStore>());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapControllers();

app.Run();
