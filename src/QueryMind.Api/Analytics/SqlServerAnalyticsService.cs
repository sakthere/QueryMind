using System.Diagnostics;
using System.Globalization;
using Microsoft.Data.SqlClient;

namespace QueryMind.Api.Analytics;

/// <summary>Executes only application-owned, parameter-free analytics statements with a read-only SQL Server login.</summary>
public sealed class SqlServerAnalyticsService(IConfiguration configuration)
{
    private const int CommandTimeoutSeconds = 10;

    private static readonly IReadOnlyDictionary<string, QueryDefinition> Queries = new Dictionary<string, QueryDefinition>
    {
        ["monthly-revenue"] = new("Monthly revenue", "Month", "Revenue", "line", """
            SELECT CONVERT(char(7), ordered_at, 120) AS [Month], SUM(total_amount) AS [Revenue]
            FROM analytics.orders WHERE status = 'completed'
            GROUP BY CONVERT(char(7), ordered_at, 120) ORDER BY [Month];
            """),
        ["category-revenue"] = new("Revenue by category", "Category", "Revenue", "bar", """
            SELECT c.name AS [Category], SUM(oi.quantity * oi.unit_price) AS [Revenue]
            FROM analytics.order_items oi JOIN analytics.products p ON p.id = oi.product_id
            JOIN analytics.categories c ON c.id = p.category_id GROUP BY c.name ORDER BY [Revenue] DESC;
            """),
        ["top-customers"] = new("Highest lifetime value customers", "Customer", "Lifetime Value", "bar", """
            SELECT TOP (5) c.full_name AS [Customer], SUM(o.total_amount) AS [Lifetime Value]
            FROM analytics.orders o JOIN analytics.customers c ON c.id = o.customer_id
            WHERE o.status = 'completed' GROUP BY c.full_name ORDER BY [Lifetime Value] DESC;
            """),
        ["regional-performance"] = new("Regional performance", "Region", "Revenue", "bar", """
            SELECT r.name AS [Region], SUM(o.total_amount) AS [Revenue]
            FROM analytics.orders o JOIN analytics.regions r ON r.id = o.region_id
            WHERE o.status = 'completed' GROUP BY r.name ORDER BY [Revenue] DESC;
            """),
        ["cancellation-rate"] = new("Order cancellation rate", "Status", "Orders", "donut", """
            SELECT status AS [Status], COUNT(*) AS [Orders]
            FROM analytics.orders GROUP BY status ORDER BY [Orders] DESC;
            """)
    };

    public static bool TryGetDefinition(string questionId, out QueryDefinition definition) => Queries.TryGetValue(questionId, out definition!);

    public async Task<AnalyticsResult> ExecuteAsync(string questionId, CancellationToken cancellationToken)
    {
        if (!Queries.TryGetValue(questionId, out var query))
            throw new KeyNotFoundException($"Unknown predefined question '{questionId}'.");

        var stopwatch = Stopwatch.StartNew();
        await using var connection = new SqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(query.Sql, connection) { CommandTimeout = CommandTimeoutSeconds };
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<IReadOnlyDictionary<string, object>>();
        var points = new List<ChartPoint>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var label = reader.GetString(0);
            var value = Convert.ToDecimal(reader.GetValue(1), CultureInfo.InvariantCulture);
            rows.Add(new Dictionary<string, object> { [query.LabelColumn] = label, [query.ValueColumn] = value });
            points.Add(new ChartPoint(label, value));
        }

        return new AnalyticsResult(questionId, query.Title, [query.LabelColumn, query.ValueColumn], rows,
            new ChartDefinition(query.ChartType, query.LabelColumn, query.ValueColumn, points), query.Sql,
            new ExecutionMetadata("SQL Server read-only query", rows.Count, stopwatch.ElapsedMilliseconds, "retail-demo-v1"));
    }

    private string GetConnectionString()
    {
        var configured = configuration.GetConnectionString("Analytics");
        if (!string.IsNullOrWhiteSpace(configured)) return configured;
        var password = Environment.GetEnvironmentVariable("ANALYTICS_READER_PASSWORD") ?? ReadDotEnvValue("ANALYTICS_READER_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Set ANALYTICS_READER_PASSWORD in .env or configure ConnectionStrings__Analytics before executing analytics.");
        return new SqlConnectionStringBuilder
        {
            DataSource = configuration["Analytics:Server"] ?? "localhost,1433",
            InitialCatalog = "QueryMindAnalytics",
            UserID = "analytics_reader",
            Password = password,
            Encrypt = true,
            TrustServerCertificate = true,
            ConnectTimeout = 5,
            ApplicationName = "QueryMind.Api"
        }.ConnectionString;
    }

    private static string? ReadDotEnvValue(string key)
    {
        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, ".env");
            if (!File.Exists(path)) continue;
            return File.ReadLines(path).Select(line => line.Split('=', 2)).FirstOrDefault(parts => parts.Length == 2 && parts[0].Trim() == key)?[1].Trim();
        }
        return null;
    }

    public sealed record QueryDefinition(string Title, string LabelColumn, string ValueColumn, string ChartType, string Sql);
}
