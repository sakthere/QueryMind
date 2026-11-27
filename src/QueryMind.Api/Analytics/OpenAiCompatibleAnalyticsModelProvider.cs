using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QueryMind.Api.Analytics;

/// <summary>Optional provider. It is activated only with explicit environment configuration; no credentials are stored in source control.</summary>
public sealed class OpenAiCompatibleAnalyticsModelProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IAnalyticsModelProvider
{
    public async Task<SqlGeneration> GenerateSqlAsync(string question, CancellationToken cancellationToken)
    {
        var baseUrl = configuration["AI:BaseUrl"]?.TrimEnd('/');
        var model = configuration["AI:Model"];
        var apiKey = configuration["AI:ApiKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(apiKey))
            return new SqlGeneration(null, null, "The external provider is not fully configured. Set AI__BASE_URL, AI__MODEL, and AI__API_KEY locally.", "openai-compatible", model ?? "unconfigured");

        var body = new
        {
            model,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = "Return JSON only: {\\\"sql\\\": string, \\\"title\\\": string}. Write exactly one SQL Server SELECT against schema analytics. Never use comments, semicolons except an optional final one, DDL, DML, system objects, or external data." },
                new { role = "user", content = question }
            }
        };
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

    public Task<AnalysisInsight> GenerateInsightAsync(AnalyticsResult result, CancellationToken cancellationToken)
        => Task.FromResult(new AnalysisInsight("Result interpretation is limited to the displayed SQL Server rows when an external provider is active.", "External-provider narrative is not enabled"));
}
