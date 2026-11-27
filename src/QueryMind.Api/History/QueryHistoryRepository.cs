using Microsoft.Data.SqlClient;
using QueryMind.Api.Analytics;

namespace QueryMind.Api.History;

public sealed record QueryHistoryEntry(Guid Id, string Question, string? Title, string Provider, string Model, string Status, int? RowCount, long? DurationMs, DateTime CreatedAt);

public sealed class QueryHistoryRepository(IConfiguration configuration)
{
    public async Task SaveAsync(string question, AnalysisResponse response, CancellationToken cancellationToken)
    {
        var password = Environment.GetEnvironmentVariable("QUERYMIND_APP_PASSWORD");
        if (string.IsNullOrWhiteSpace(password)) return;
        await using var connection = new SqlConnection(ConnectionString(password));
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            INSERT INTO dbo.query_history (id, question, title, provider, model, status, row_count, duration_ms)
            VALUES (@id, @question, @title, @provider, @model, @status, @rows, @duration);
            """, connection);
        command.Parameters.AddWithValue("@id", Guid.NewGuid());
        command.Parameters.AddWithValue("@question", question.Trim());
        command.Parameters.AddWithValue("@title", (object?)response.Result?.Title ?? DBNull.Value);
        command.Parameters.AddWithValue("@provider", response.Provider);
        command.Parameters.AddWithValue("@model", response.Model);
        command.Parameters.AddWithValue("@status", response.Status);
        command.Parameters.AddWithValue("@rows", (object?)response.Result?.Metadata.RowCount ?? DBNull.Value);
        command.Parameters.AddWithValue("@duration", (object?)response.Result?.Metadata.DurationMs ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<QueryHistoryEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var password = Environment.GetEnvironmentVariable("QUERYMIND_APP_PASSWORD");
        if (string.IsNullOrWhiteSpace(password)) return [];
        var entries = new List<QueryHistoryEntry>();
        await using var connection = new SqlConnection(ConnectionString(password));
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("SELECT TOP (50) id, question, title, provider, model, status, row_count, duration_ms, created_at FROM dbo.query_history ORDER BY created_at DESC;", connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            entries.Add(new QueryHistoryEntry(reader.GetGuid(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetInt32(6), reader.IsDBNull(7) ? null : reader.GetInt64(7), reader.GetDateTime(8)));
        return entries;
    }

    private string ConnectionString(string password) => new SqlConnectionStringBuilder
    {
        DataSource = configuration["AppDatabase:Server"] ?? "localhost,1433", InitialCatalog = "QueryMindApp", UserID = "querymind_app", Password = password,
        Encrypt = true, TrustServerCertificate = true, ConnectTimeout = 5, ApplicationName = "QueryMind.Api"
    }.ConnectionString;
}
