using Microsoft.AspNetCore.Mvc;
using QueryMind.Api.Evaluation;

namespace QueryMind.Api.Controllers;

[ApiController]
[Route("api/evaluations")]
public sealed class EvaluationsController(EvaluationService evaluations) : ControllerBase
{
    [HttpPost("run")]
    public async Task<ActionResult<EvaluationRunResult>> Run(CancellationToken cancellationToken)
        => Ok(await evaluations.RunAsync(cancellationToken));
}
