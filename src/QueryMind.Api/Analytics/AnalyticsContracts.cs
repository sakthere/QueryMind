namespace QueryMind.Api.Analytics;
public sealed record SuggestedQuestion(string Id, string Label, string Description, string Category);
public sealed record ChartPoint(string Label, decimal Value, string? Series = null);
public sealed record ChartDefinition(string Type, string XAxis, string YAxis, IReadOnlyList<ChartPoint> Points);
public sealed record ExecutionMetadata(string Source, int RowCount, long DurationMs, string DatasetVersion);
public sealed record AnalyticsResult(string QuestionId, string Title, IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyDictionary<string, object>> Rows, ChartDefinition Chart, string SqlPreview, ExecutionMetadata Metadata);

public sealed record AnalyzeQuestionRequest(string Question);
public sealed record PipelineStage(string Name, string Status, string Detail);
public sealed record AnalysisInsight(string Summary, string Source);
public sealed record AnalysisResponse(
    string Status,
    string? Clarification,
    AnalyticsResult? Result,
    IReadOnlyList<PipelineStage> Stages,
    IReadOnlyList<AnalysisInsight> Insights,
    string Provider,
    string Model,
    int Attempts);

public sealed record SqlGeneration(string? Sql, string? Title, string? Clarification, string Provider, string Model);
public interface IAnalyticsModelProvider
{
    Task<SqlGeneration> GenerateSqlAsync(string question, CancellationToken cancellationToken);
    Task<AnalysisInsight> GenerateInsightAsync(AnalyticsResult result, CancellationToken cancellationToken);
}
