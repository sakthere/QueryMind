using System.Diagnostics;
using QueryMind.Api.Analytics;

namespace QueryMind.Api.Evaluation;

public sealed record EvaluationCaseResult(string Id, string Question, bool Passed, string Detail, long DurationMs);
public sealed record EvaluationRunResult(DateTimeOffset RanAt, string Provider, int Total, int Passed, int Failed, IReadOnlyList<EvaluationCaseResult> Cases);

/// <summary>
/// Deterministic regression checks for the local reviewed provider. It never invokes an external model.
/// </summary>
public sealed class EvaluationService(SafeSqlValidator validator)
{
    private static readonly (string Id, string Question, string ExpectedSql)[] Cases =
    [
        ("revenue-trend", "How has monthly revenue changed?", "analytics.orders"),
        ("category-comparison", "Compare revenue across product categories", "analytics.order_items"),
        ("customer-value", "Which customers have the highest lifetime value?", "analytics.customers"),
        ("regional-performance", "Which regions are performing best?", "analytics.regions"),
        ("order-status", "What percentage of orders were cancelled?", "GROUP BY status")
    ];

    public async Task<EvaluationRunResult> RunAsync(CancellationToken cancellationToken)
    {
        var provider = new RuleBasedAnalyticsModelProvider();
        var results = new List<EvaluationCaseResult>();
        foreach (var item in Cases)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                var generated = await provider.GenerateSqlAsync(item.Question, cancellationToken);
                if (generated.Sql is null) throw new InvalidOperationException(generated.Clarification ?? "No SQL was returned.");
                validator.Validate(generated.Sql);
                var passed = generated.Sql.Contains(item.ExpectedSql, StringComparison.OrdinalIgnoreCase);
                results.Add(new EvaluationCaseResult(item.Id, item.Question, passed, passed ? "Expected query shape and safety validation passed." : "Query did not match the expected shape.", watch.ElapsedMilliseconds));
            }
            catch (Exception exception)
            {
                results.Add(new EvaluationCaseResult(item.Id, item.Question, false, exception.Message, watch.ElapsedMilliseconds));
            }
        }
        return new EvaluationRunResult(DateTimeOffset.UtcNow, "reviewed-template/local-rules-v1", results.Count, results.Count(result => result.Passed), results.Count(result => !result.Passed), results);
    }
}
