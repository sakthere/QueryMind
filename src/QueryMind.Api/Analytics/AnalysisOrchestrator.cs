namespace QueryMind.Api.Analytics;

/// <summary>Coordinates question interpretation, AST validation, restricted execution, and attributable insight generation.</summary>
public sealed class AnalysisOrchestrator(
    IAnalyticsModelProvider modelProvider,
    SafeSqlValidator sqlValidator,
    SqlServerAnalyticsService analytics)
{
    public async Task<AnalysisResponse> AnalyzeAsync(AnalyzeQuestionRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question) || request.Question.Trim().Length > 500)
            return new AnalysisResponse("needs-clarification", "Ask one analytics question using 1 to 500 characters.", null,
                [new PipelineStage("Question", "blocked", "A concise question is required.")], [], "none", "none", 0);

        var generated = await modelProvider.GenerateSqlAsync(request.Question, cancellationToken);
        var stages = new List<PipelineStage> { new("Interpretation", "complete", $"Provider: {generated.Provider}") };
        if (generated.Clarification is not null || generated.Sql is null)
            return new AnalysisResponse("needs-clarification", generated.Clarification ?? "Please refine the question.", null, stages,
                [], generated.Provider, generated.Model, 0);

        try
        {
            sqlValidator.Validate(generated.Sql);
            stages.Add(new PipelineStage("SQL safety", "complete", "Parsed as one approved read-only SELECT."));

            // The local provider selects a reviewed template. A later external provider will use the same AST gate
            // and a restricted generic executor before any SQL Server command is issued.
            var result = await ExecuteReviewedTemplateAsync(generated.Sql, cancellationToken);
            stages.Add(new PipelineStage("SQL Server", "complete", $"Returned {result.Metadata.RowCount} rows."));
            var insight = await modelProvider.GenerateInsightAsync(result, cancellationToken);
            stages.Add(new PipelineStage("Insight", "complete", insight.Source));
            return new AnalysisResponse("complete", null, result, stages, [insight], generated.Provider, generated.Model, 0);
        }
        catch (SqlSafetyException exception)
        {
            stages.Add(new PipelineStage("SQL safety", "blocked", exception.Message));
            return new AnalysisResponse("blocked", exception.Message, null, stages, [], generated.Provider, generated.Model, 0);
        }
    }

    private Task<AnalyticsResult> ExecuteReviewedTemplateAsync(string sql, CancellationToken cancellationToken)
    {
        var match = new[] { "monthly-revenue", "category-revenue", "top-customers", "regional-performance", "cancellation-rate" }
            .FirstOrDefault(id => SqlServerAnalyticsService.TryGetDefinition(id, out var definition) && string.Equals(definition.Sql.Trim(), sql.Trim(), StringComparison.Ordinal));
        if (match is null) throw new SqlSafetyException("Only reviewed query templates can be executed while the local provider is active.");
        return analytics.ExecuteAsync(match, cancellationToken);
    }
}
