using Microsoft.AspNetCore.Mvc;
using QueryMind.Api.Analytics;
namespace QueryMind.Api.Controllers;
[ApiController, Route("api/analytics")]
public sealed class AnalyticsController(DemoAnalyticsService analytics, SqlServerAnalyticsService sqlServerAnalytics) : ControllerBase
{
    [HttpGet("questions")]
    public ActionResult<IReadOnlyList<SuggestedQuestion>> GetQuestions() => Ok(analytics.GetQuestions());

    [HttpPost("execute/{questionId}")]
    public async Task<ActionResult<AnalyticsResult>> Execute(string questionId, CancellationToken cancellationToken)
        => Ok(await sqlServerAnalytics.ExecuteAsync(questionId, cancellationToken));
}
