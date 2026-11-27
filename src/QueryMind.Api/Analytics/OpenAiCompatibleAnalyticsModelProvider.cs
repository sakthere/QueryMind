using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using QueryMind.Api.Schema;

namespace QueryMind.Api.Analytics;

/// <summary>Optional provider. It is activated only with explicit environment configuration; no credentials are stored in source control.</summary>
public sealed class OpenAiCompatibleAnalyticsModelProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, SchemaDiscoveryService schema) : IAnalyticsModelProvider
{
    public async Task<SqlGeneration> GenerateSqlAsync(string question, CancellationToken cancellationToken)
    {
        var baseUrl = configuration["AI:BaseUrl"]?.TrimEnd('/');
        var model = configuration["AI:Model"];
        var apiKey = configuration["AI:ApiKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(apiKey))
            return new SqlGeneration(null, null, "The external provider is not fully configured. Set AI__BASE_URL, AI__MODEL, and AI__API_KEY locally.", "openai-compatible", model ?? "unconfigured");

        var body = RequestBody(model, question, null);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        using var response = await httpClientFactory.CreateClient("analytics-model").SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return new SqlGeneration(null, null, "The configured model provider did not return an analysis.", "openai-compatible", model);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var content = document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        using var generated = JsonDocument.Parse(content ?? "{}");
        var root = generated.RootElement;
        return new SqlGeneration(root.TryGetProperty("sql", out var sql) ? sql.GetString() : null,
            root.TryGetProperty("title", out var title) ? title.GetString() : "Analysis result", null, "openai-compatible", model);
    }

    public async Task<SqlGeneration> RepairSqlAsync(string question, string previousSql, string failureReason, CancellationToken cancellationToken)
    {
        var baseUrl = configuration["AI:BaseUrl"]?.TrimEnd('/');
        var model = configuration["AI:Model"];
        var apiKey = configuration["AI:ApiKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(apiKey))
            return new SqlGeneration(null, null, "The external provider is not fully configured.", "openai-compatible", model ?? "unconfigured");
        var body = RequestBody(model, question, $"The prior SQL was rejected: {failureReason}. Previous SQL: {previousSql}");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions") { Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        using var response = await httpClientFactory.CreateClient("analytics-model").SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return new SqlGeneration(null, null, "The configured model provider could not repair the query.", "openai-compatible", model);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var content = document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        using var generated = JsonDocument.Parse(content ?? "{}");
        return new SqlGeneration(generated.RootElement.TryGetProperty("sql", out var sql) ? sql.GetString() : null, generated.RootElement.TryGetProperty("title", out var title) ? title.GetString() : "Analysis result", null, "openai-compatible", model);
    }

    private object RequestBody(string model, string question, string? repair)
    {
        var schemaContext = string.Join("; ", schema.GetSchema().Select(table => $"analytics.{table.Name} ({string.Join(", ", table.Columns.Select(column => column.Name))})"));
        return new { model, response_format = new { type = "json_object" }, messages = new[]
        {
            new { role = "system", content = $"Return JSON only: {{\\\"sql\\\": string, \\\"title\\\": string}}. Write exactly one SQL Server SELECT. Use only this curated schema: {schemaContext}. Never use comments, DDL, DML, system objects, external data, or SELECT INTO." },
            new { role = "user", content = repair is null ? question : $"{question}\n{repair}" }
        }};
    }

    public Task<AnalysisInsight> GenerateInsightAsync(AnalyticsResult result, CancellationToken cancellationToken)
        => Task.FromResult(new AnalysisInsight("Result interpretation is limited to the displayed SQL Server rows when an external provider is active.", "External-provider narrative is not enabled"));
}
