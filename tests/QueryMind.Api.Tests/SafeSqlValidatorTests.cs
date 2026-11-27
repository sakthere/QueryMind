using QueryMind.Api.Analytics;

namespace QueryMind.Api.Tests;

public sealed class SafeSqlValidatorTests
{
    private readonly SafeSqlValidator _validator = new();

    [Fact]
    public void Validate_accepts_one_select_over_an_approved_table()
    {
        var validated = _validator.Validate("SELECT TOP (5) status, COUNT(*) AS orders FROM analytics.orders GROUP BY status;");
        Assert.Contains("analytics.orders", validated.Sql);
    }

    [Theory]
    [InlineData("DELETE FROM analytics.orders;")]
    [InlineData("SELECT * FROM dbo.users;")]
    [InlineData("SELECT * FROM analytics.orders; SELECT * FROM analytics.customers;")]
    [InlineData("SELECT * INTO #copy FROM analytics.orders;")]
    public void Validate_rejects_unsafe_or_unapproved_sql(string sql)
    {
        Assert.Throws<SqlSafetyException>(() => _validator.Validate(sql));
    }
}
