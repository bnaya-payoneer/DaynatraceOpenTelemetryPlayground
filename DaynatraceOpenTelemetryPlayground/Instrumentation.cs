// credit: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Instrumentation.cs

namespace Bnaya.Examples;

using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <summary>
/// It is recommended to use a custom type to hold references for
/// ActivitySource and Instruments. This avoids possible type collisions
/// with other components in the DI container.
/// </summary>
public class Instrumentation : IInstrumentation
{
    internal const string ActivitySourceName = "Bnaya.Examples.Tracing";
    internal const string MeterName = "Bnaya.Examples.Metrics";
    private readonly Meter _meter;

    public Instrumentation()
    {
        string? version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
        TraceFactory = new ActivitySource(ActivitySourceName, version);
        _meter = new Meter(MeterName, version);
        Mod3 = this._meter.CreateCounter<long>("mod.3", "When modulo by 3 == 0");
    }

    public ActivitySource TraceFactory { get; }

    public Counter<long> Mod3 { get; }

    public void Dispose()
    {
        this.TraceFactory.Dispose();
        this._meter.Dispose();
    }
}