using QueryMind.Api.Analytics;

namespace QueryMind.Api.Tests;

public sealed class DemoAnalyticsServiceTests
{
    [Fact]
    public void Execute_monthly_revenue_returns_ordered_fixture_rows()
    {
        var result = new DemoAnalyticsService().Execute("monthly-revenue");

        Assert.Equal("line", result.Chart.Type);
        Assert.Equal(12, result.Rows.Count);
        Assert.Contains("GROUP BY", result.SqlPreview);
    }

    [Fact]
    public void Execute_rejects_unknown_question()
    {
        Assert.Throws<KeyNotFoundException>(() => new DemoAnalyticsService().Execute("drop-all-tables"));
    }
}
