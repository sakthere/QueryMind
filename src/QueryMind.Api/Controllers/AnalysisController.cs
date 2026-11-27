using Microsoft.AspNetCore.Mvc;
using QueryMind.Api.Analytics;

namespace QueryMind.Api.Controllers;

[ApiController]
[Route("api/analysis")]
public sealed class AnalysisController(AnalysisOrchestrator orchestrator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AnalysisResponse>> Analyze([FromBody] AnalyzeQuestionRequest request, CancellationToken cancellationToken)
        => Ok(await orchestrator.AnalyzeAsync(request, cancellationToken));
}
