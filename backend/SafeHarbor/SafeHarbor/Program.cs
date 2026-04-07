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

=======
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

// Using Scoped now that we have a real DbContext integrated
builder.Services.AddScoped<InMemoryDataStore>(); 
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
>>>>>>> 1055952cf0593aa6d7cb59113f4108591b1e3ecd
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