// credit: https://opentelemetry.io/docs/instrumentation/net/getting-started/

using AspireDashboard.Extensions;
using Bnaya.Examples;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Globalization;

const string serviceName = "roll-dice";

var builder = WebApplication.CreateBuilder(args);

// *************************************************************************

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
        .AddOtlpExporter();
        //.AddConsoleExporter();
});
builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource =>
            resource.AddService(serviceName)
                    .AddAttributes(new[] { KeyValuePair.Create<string, object>("custom-value", 42) }))
      .WithTracing(tracing => tracing
          .AddSource(Instrumentation.ActivitySourceName)
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
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
          .AddMeter(Instrumentation.MeterName)
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          //.AddConsoleExporter()
          .AddOtlpExporter()
          //.AddOtlpExporter(options =>
          //{
          //    options.Endpoint = new Uri(Environment.GetEnvironmentVariable("DT_API_URL") + "/v1/traces");
          //    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
          //    options.Headers = $"Authorization=Api-Token {Environment.GetEnvironmentVariable("DT_API_TOKEN")}";
          //})
          .AddPrometheusExporter());

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
    instrumentation.Mod3.Add(result);

    if (string.IsNullOrEmpty(player))
    {
        logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
    }
    else
    {
        logger.LogInformation("{player} is rolling the dice: {result}", player, result);
    }

    await Task.Delay(result * 100);
    using var trcIn = instrumentation.TraceFactory.StartActivity("bnaya.rolling.internal");
    logger.LogInformation("Internal span: {result}", result);
    await Task.Delay((12 - result) * 100);

    return result.ToString(CultureInfo.InvariantCulture);
}

app.MapGet("/rolldice/{player?}", HandleRollDice);

app.Run();

static int RollDice() => Environment.TickCount % 12;
