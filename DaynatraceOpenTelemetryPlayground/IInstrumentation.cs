// credit: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Instrumentation.cs

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Bnaya.Examples;
public interface IInstrumentation: IDisposable
{
    Counter<long> Mod3 { get; }
    ActivitySource TraceFactory { get; }
}