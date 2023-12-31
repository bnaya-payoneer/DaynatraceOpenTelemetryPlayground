// credit: https://opentelemetry.io/docs/instrumentation/net/getting-started/

using AspireDashboard.Extensions;
using Bnaya.Examples;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Globalization;
using System.Reflection;

const string serviceName = "roll-dice";

var builder = WebApplication.CreateBuilder(args);

// *************************************************************************

string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
var honeycombApiKey  = Environment.GetEnvironmentVariable("Honeycomb-API-Key") ?? throw new NullReferenceException();

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
        .AddOtlpExporter();
        //.AddConsoleExporter();
});
#pragma warning disable CS8604 // Possible null reference argument.

builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource =>
            resource.AddService(serviceName)
                    //.AddAttributes(new[] {
                    //    KeyValuePair.Create<string, object>("K8s_POD_NAME", 
                    //            Environment.GetEnvironmentVariable("K8s_POD_NAME")),
                    //    KeyValuePair.Create<string, object>("K8s_NS", 
                    //            Environment.GetEnvironmentVariable("K8s_NS")),
                    //    KeyValuePair.Create<string, object>("APP_VERSION",
                    //            Assembly.GetExecutingAssembly().GetName().Version)
                    //})
                    )

      .WithTracing(tracing => tracing
          .ConfigureResource(resource => resource.AddService(serviceName))
          .AddSource(Instrumentation.ActivitySourceName)
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddGrpcClientInstrumentation()
          //.AddHoneycomb(cfg =>
          //{
          //    cfg.ApiKey = honeycombApiKey;
          //    cfg.ServiceName = serviceName;
          //    cfg.ServiceVersion = version;
          //})
          //.AddRedisInstrumentation()
          //.AddSqlClientInstrumentation()
          .AddOtlpExporter()
          //.AddConsoleExporter()
          //.AddOtlpExporter(options =>
          //{
          //    options.Endpoint = new Uri(Environment.GetEnvironmentVariable("DT_API_URL") + "/v1/traces");
          //    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
          //    options.Headers = $"Authorization=Api-Token {Environment.GetEnvironmentVariable("DT_API_TOKEN")}";
          //})
          .SetSampler<AlwaysOnSampler>())
      .WithMetrics(metrics => metrics
          .ConfigureResource(resource => resource.AddService(serviceName))
          .AddMeter(Instrumentation.MeterName)
          //.AddHoneycomb(cfg =>
          //{
          //    cfg.ApiKey = honeycombApiKey;
          //    cfg.ServiceName = serviceName;
          //    cfg.ServiceVersion = version;
          //})
          //.AddAspNetCoreInstrumentation()
          //.AddHttpClientInstrumentation()
          .AddConsoleExporter()
          .AddOtlpExporter()
          //.AddOtlpExporter(options =>
          //{
          //    options.Endpoint = new Uri(Environment.GetEnvironmentVariable("DT_API_URL") + "/v1/traces");
          //    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
          //    options.Headers = $"Authorization=Api-Token {Environment.GetEnvironmentVariable("DT_API_TOKEN")}";
          //})
          .AddPrometheusExporter());
#pragma warning restore CS8604 // Possible null reference argument.

builder.Services.AddSingleton<IInstrumentation, Instrumentation>();
builder.Services.AddAspireDashboard();

// *************************************************************************

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseStaticFiles(); // needed for aspire dashboard

// *************************************************************************
// https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Prometheus.AspNetCore/README.md
app.UseOpenTelemetryPrometheusScrapingEndpoint();
// *************************************************************************

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

async Task<string> HandleRollDice(
            [FromServices] ILogger<Program> logger,
            [FromServices] IInstrumentation instrumentation,
            string? player)
{
    using var trc = instrumentation.TraceFactory.StartActivity("bnaya.rolling");
    var result = RollDice();
    instrumentation.Metrics.CustomMetrics1.Add(result);
    instrumentation.Metrics.CustomMetrics2.Add(result % 2 == 0 ? result * 2 : -result * 2);
    instrumentation.Metrics.CustomMetrics3.Add(result);
        
    if (string.IsNullOrEmpty(player))
    {
        logger.LogAnonymous(result);
    }
    else
    {
        logger.LogPlayer(player, result);
    }

    await Task.Delay(result * 100);
    using var trcIn = instrumentation.TraceFactory.StartActivity("bnaya.rolling.internal");
    logger.LogInternal(result);
    await Task.Delay(result * 10);
    instrumentation.Metrics.CustomMetrics1.Add(result);
    instrumentation.EventFactory.Write("pin");
    await Task.Delay((12 - result) * 100);

    return result.ToString(CultureInfo.InvariantCulture);
}

app.MapGet("/rolldice/{player?}", HandleRollDice);

app.Run();

static int RollDice() => Environment.TickCount % 12;
