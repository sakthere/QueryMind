using System.Collections.Concurrent;

namespace QueryMind.Api.Observability;

public sealed record AnalysisMetric(DateTimeOffset OccurredAt, string Provider, string Outcome, int Attempts, long DurationMs, int? RowCount);
public sealed record AnalysisMetricsSummary(int Total, int Completed, int Blocked, int Clarifications, double AverageDurationMs, IReadOnlyDictionary<string, int> ByProvider);

/// <summary>In-memory, non-sensitive operational metrics. Questions, SQL, result values, and secrets are never recorded.</summary>
public sealed class AnalysisMetrics
{
    private readonly ConcurrentQueue<AnalysisMetric> _events = new();
    private const int Capacity = 500;

    public void Record(string provider, string outcome, int attempts, long durationMs, int? rowCount)
    {
        _events.Enqueue(new AnalysisMetric(DateTimeOffset.UtcNow, provider, outcome, attempts, durationMs, rowCount));
        while (_events.Count > Capacity && _events.TryDequeue(out _)) { }
    }

    public AnalysisMetricsSummary Summary()
    {
        var events = _events.ToArray();
        return new AnalysisMetricsSummary(events.Length, events.Count(item => item.Outcome == "complete"), events.Count(item => item.Outcome == "blocked"), events.Count(item => item.Outcome == "needs-clarification"), events.Length == 0 ? 0 : Math.Round(events.Average(item => item.DurationMs), 1), events.GroupBy(item => item.Provider).ToDictionary(group => group.Key, group => group.Count()));
    }
}
