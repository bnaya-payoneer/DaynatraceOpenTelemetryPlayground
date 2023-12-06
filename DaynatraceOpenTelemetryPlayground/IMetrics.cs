// credit: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Instrumentation.cs

using System.Diagnostics.Metrics;

namespace Bnaya.Examples;
public interface IMetrics
{
    Counter<long> CustomMetrics { get; }
}   