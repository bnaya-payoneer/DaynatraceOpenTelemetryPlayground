// credit: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Instrumentation.cs

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;

namespace Bnaya.Examples;
public interface IInstrumentation: IDisposable
{
    IMetrics Metrics { get; }
    ActivitySource TraceFactory { get; }
    EventSource EventFactory { get; }
}