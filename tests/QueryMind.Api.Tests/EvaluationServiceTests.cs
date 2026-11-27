using QueryMind.Api.Analytics;
using QueryMind.Api.Evaluation;

namespace QueryMind.Api.Tests;

public sealed class EvaluationServiceTests
{
    [Fact]
    public async Task Run_reports_all_local_regression_cases_as_passing()
    {
        var result = await new EvaluationService(new SafeSqlValidator()).RunAsync(CancellationToken.None);
        Assert.Equal(5, result.Total);
        Assert.Equal(5, result.Passed);
        Assert.Equal(0, result.Failed);
        Assert.All(result.Cases, item => Assert.True(item.Passed, item.Detail));
    }
}
