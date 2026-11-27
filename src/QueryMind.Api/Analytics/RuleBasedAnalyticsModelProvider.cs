namespace QueryMind.Api.Analytics;

/// <summary>
/// Local provider used until an external model is configured. It only selects reviewed query templates;
/// it never treats user text as executable SQL.
/// </summary>
public sealed class RuleBasedAnalyticsModelProvider : IAnalyticsModelProvider
{
    public Task<SqlGeneration> GenerateSqlAsync(string question, CancellationToken cancellationToken)
    {
        var normalised = question.Trim().ToLowerInvariant();
        var questionId = normalised.Contains("customer") ? "top-customers"
            : normalised.Contains("region") ? "regional-performance"
            : normalised.Contains("cancel") || normalised.Contains("status") ? "cancellation-rate"
            : normalised.Contains("category") || normalised.Contains("product") ? "category-revenue"
            : normalised.Contains("revenue") || normalised.Contains("sales") || normalised.Contains("month") ? "monthly-revenue"
            : null;

        if (questionId is null || !SqlServerAnalyticsService.TryGetDefinition(questionId, out var definition))
            return Task.FromResult(new SqlGeneration(null, null,
                "Try asking about monthly revenue, category revenue, top customers, regional performance, or order status.",
                "reviewed-template", "local-rules-v1"));

        return Task.FromResult(new SqlGeneration(definition.Sql, definition.Title, null, "reviewed-template", "local-rules-v1"));
    }

    public Task<AnalysisInsight> GenerateInsightAsync(AnalyticsResult result, CancellationToken cancellationToken)
    {
        var highest = result.Chart.Points.OrderByDescending(point => point.Value).FirstOrDefault();
        var summary = highest is null
            ? "The approved query returned no rows."
            : $"{highest.Label} is the largest displayed value at {highest.Value:N2}.";
        return Task.FromResult(new AnalysisInsight(summary, "Deterministic summary from returned SQL Server rows"));
    }

    public Task<SqlGeneration> RepairSqlAsync(string question, string previousSql, string failureReason, CancellationToken cancellationToken)
        => GenerateSqlAsync(question, cancellationToken);
}
