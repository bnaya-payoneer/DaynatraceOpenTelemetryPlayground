// credit: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Instrumentation.cs

namespace Bnaya.Examples;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;

/// <summary>
/// It is recommended to use a custom type to hold references for
/// ActivitySource and Instruments. This avoids possible type collisions
/// with other components in the DI container.
/// </summary>
public class Instrumentation : IInstrumentation
{
    internal const string ActivitySourceName = "Bnaya.Examples.Tracing";
    internal const string EventSourceName = "Bnaya.Examples.Event";
    internal const string MeterName = "Bnaya.Examples.Metrics";
    private readonly Meter _meter;

    public Instrumentation()
    {
        string? version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
        TraceFactory = new ActivitySource(ActivitySourceName, version);
        EventFactory = new EventSource(EventSourceName);
        _meter = new Meter(MeterName, version);
        Metrics = new MetricsInternal(_meter);
    }

    public ActivitySource TraceFactory { get; }
    public EventSource EventFactory { get; }

    public IMetrics Metrics { get; }

    public class MetricsInternal : IMetrics
    {
        internal MetricsInternal(Meter meter)
        {
            CustomMetrics = meter.CreateCounter<long>("my.custom.metrics", "Custom one");
        }
        public Counter<long> CustomMetrics { get; }
    }

    public void Dispose()
    {
        this.TraceFactory.Dispose();
        this._meter.Dispose();
    }
}
