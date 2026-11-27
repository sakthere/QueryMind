using Microsoft.AspNetCore.Mvc;
using QueryMind.Api.Schema;
namespace QueryMind.Api.Controllers;
[ApiController, Route("api/schema")]
public sealed class SchemaController(SchemaDiscoveryService schema) : ControllerBase { [HttpGet] public ActionResult<IReadOnlyList<SchemaTable>> Get() => Ok(schema.GetSchema()); }
