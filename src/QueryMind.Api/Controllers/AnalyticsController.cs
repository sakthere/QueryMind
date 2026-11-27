using Microsoft.AspNetCore.Mvc;
using QueryMind.Api.Analytics;
namespace QueryMind.Api.Controllers;
[ApiController, Route("api/analytics")]
public sealed class AnalyticsController(DemoAnalyticsService analytics) : ControllerBase { [HttpGet("questions")] public ActionResult<IReadOnlyList<SuggestedQuestion>> GetQuestions() => Ok(analytics.GetQuestions()); [HttpPost("execute/{questionId}")] public ActionResult<AnalyticsResult> Execute(string questionId) => Ok(analytics.Execute(questionId)); }
