namespace QueryMind.Api.Analytics;
public sealed record SuggestedQuestion(string Id, string Label, string Description, string Category);
public sealed record ChartPoint(string Label, decimal Value, string? Series = null);
public sealed record ChartDefinition(string Type, string XAxis, string YAxis, IReadOnlyList<ChartPoint> Points);
public sealed record ExecutionMetadata(string Source, int RowCount, long DurationMs, string DatasetVersion);
public sealed record AnalyticsResult(string QuestionId, string Title, IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyDictionary<string, object>> Rows, ChartDefinition Chart, string SqlPreview, ExecutionMetadata Metadata);
