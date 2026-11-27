namespace QueryMind.Api.Analytics;
using QueryMind.Api.History;
using QueryMind.Api.Observability;

/// <summary>Coordinates question interpretation, AST validation, restricted execution, and attributable insight generation.</summary>
public sealed class AnalysisOrchestrator(
    IAnalyticsModelProvider modelProvider,
    SafeSqlValidator sqlValidator,
    SqlServerAnalyticsService analytics,
    QueryHistoryRepository history,
    AnalysisMetrics metrics)
{
    public async Task<AnalysisResponse> AnalyzeAsync(AnalyzeQuestionRequest request, CancellationToken cancellationToken)
    {
        var started = System.Diagnostics.Stopwatch.StartNew();
        if (string.IsNullOrWhiteSpace(request.Question) || request.Question.Trim().Length > 500)
        {
            var invalid = new AnalysisResponse("needs-clarification", "Ask one analytics question using 1 to 500 characters.", null,
                [new PipelineStage("Question", "blocked", "A concise question is required.")], [], "none", "none", 0);
            return Record(invalid, started);
        }

        var generated = await modelProvider.GenerateSqlAsync(request.Question, cancellationToken);
        var stages = new List<PipelineStage> { new("Interpretation", "complete", $"Provider: {generated.Provider}") };
        if (generated.Clarification is not null || generated.Sql is null)
            return Record(new AnalysisResponse("needs-clarification", generated.Clarification ?? "Please refine the question.", null, stages,
                [], generated.Provider, generated.Model, 0), started);

        try
        {
            var validated = sqlValidator.Validate(generated.Sql);
            stages.Add(new PipelineStage("SQL safety", "complete", "Parsed as one approved read-only SELECT."));
            var result = await analytics.ExecuteValidatedAsync(validated, generated.Title ?? "Analysis result", cancellationToken);
            stages.Add(new PipelineStage("SQL Server", "complete", $"Returned {result.Metadata.RowCount} rows."));
            var insight = await modelProvider.GenerateInsightAsync(result, cancellationToken);
            stages.Add(new PipelineStage("Insight", "complete", insight.Source));
            var response = new AnalysisResponse("complete", null, result, stages, [insight], generated.Provider, generated.Model, 0);
            await SaveHistoryAsync(request.Question, response, cancellationToken);
            return Record(response, started);
        }
        catch (SqlSafetyException exception)
        {
            var repair = await modelProvider.RepairSqlAsync(request.Question, generated.Sql, exception.Message, cancellationToken);
            if (!string.IsNullOrWhiteSpace(repair.Sql) && !string.Equals(repair.Sql.Trim(), generated.Sql.Trim(), StringComparison.Ordinal))
            {
                try
                {
                    var validated = sqlValidator.Validate(repair.Sql);
                    stages.Add(new PipelineStage("SQL repair", "complete", "One bounded repair passed the safety gate."));
                    var result = await analytics.ExecuteValidatedAsync(validated, repair.Title ?? "Analysis result", cancellationToken);
                    var insight = await modelProvider.GenerateInsightAsync(result, cancellationToken);
                    var response = new AnalysisResponse("complete", null, result, stages, [insight], repair.Provider, repair.Model, 1);
                    await SaveHistoryAsync(request.Question, response, cancellationToken);
                    return Record(response, started);
                }
                catch (SqlSafetyException repairFailure) { exception = repairFailure; }
            }
            stages.Add(new PipelineStage("SQL safety", "blocked", exception.Message));
            return Record(new AnalysisResponse("blocked", exception.Message, null, stages, [], generated.Provider, generated.Model, 1), started);
        }
    }

    private AnalysisResponse Record(AnalysisResponse response, System.Diagnostics.Stopwatch started)
    {
        metrics.Record(response.Provider, response.Status, response.Attempts, started.ElapsedMilliseconds, response.Result?.Metadata.RowCount);
        return response;
    }

    private async Task SaveHistoryAsync(string question, AnalysisResponse response, CancellationToken cancellationToken)
    {
        try { await history.SaveAsync(question, response, cancellationToken); }
        catch { /* History must never change an analytics answer into a failure. */ }
    }

}
