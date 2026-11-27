using Microsoft.AspNetCore.Mvc;
using QueryMind.Api.History;

namespace QueryMind.Api.Controllers;

[ApiController]
[Route("api/history")]
public sealed class HistoryController(QueryHistoryRepository history) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QueryHistoryEntry>>> List(CancellationToken cancellationToken)
        => Ok(await history.ListAsync(cancellationToken));
}
