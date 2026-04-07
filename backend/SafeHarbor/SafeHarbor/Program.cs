using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // Added for UseSqlServer
using Microsoft.Identity.Web;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SafeHarbor.Authorization;
using SafeHarbor.Data; // Added to access your new SafeHarborDbContext
using SafeHarbor.Infrastructure;
using SafeHarbor.Services;

var builder = WebApplication.CreateBuilder(args);

// NOTE: JSON console formatting keeps logs structured for Azure Log Analytics and App Insights queries.
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
});

// --- DATABASE REGISTRATION START ---
// This connects your 17 tables to the SQL Connection String in appsettings.json
builder.Services.AddDbContext<SafeHarborDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// --- DATABASE REGISTRATION END ---

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.AdminOnly, policy => policy.RequireRole("Admin"));
});

// TODO: Chad - transition these from Singleton to Scoped now that we have a DbContext
builder.Services.AddScoped<InMemoryDataStore>(); 
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
builder.Services.AddSingleton<IDataRetentionRedactionService, DataRetentionRedactionService>();

builder.Services.AddHealthChecks();

// Telemetry configuration...
var telemetryServiceName = builder.Configuration["Telemetry:ServiceName"] ?? "safeharbor-api";
var otlpEndpoint = builder.Configuration["Telemetry:OtlpEndpoint"];

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(telemetryServiceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation(); // <--- Added semicolon here to close the chain!
            // .AddEntityFrameworkCoreInstrumentation(); // This stays commented out for now

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapControllers();

app.Run();