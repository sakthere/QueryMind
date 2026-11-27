using Microsoft.AspNetCore.Mvc;
using QueryMind.Api.Observability;

namespace QueryMind.Api.Controllers;

[ApiController]
[Route("api/observability")]
public sealed class ObservabilityController(AnalysisMetrics metrics) : ControllerBase
{
    [HttpGet("summary")]
    public ActionResult<AnalysisMetricsSummary> Summary() => Ok(metrics.Summary());
}
