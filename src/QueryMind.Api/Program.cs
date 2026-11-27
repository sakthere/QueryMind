using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using QueryMind.Api.Analytics;
using QueryMind.Api.Schema;
using QueryMind.Api.History;
using QueryMind.Api.Evaluation;
using QueryMind.Api.Observability;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("analytics", limiter => { limiter.PermitLimit = 30; limiter.Window = TimeSpan.FromMinutes(1); limiter.QueueLimit = 0; }));
builder.Services.AddSingleton<DemoAnalyticsService>();
builder.Services.AddSingleton<SqlServerAnalyticsService>();
builder.Services.AddSingleton<SafeSqlValidator>();
builder.Services.AddHttpClient("analytics-model", client => client.Timeout = TimeSpan.FromSeconds(20));
if (string.Equals(builder.Configuration["AI:Provider"], "openai-compatible", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddSingleton<IAnalyticsModelProvider, OpenAiCompatibleAnalyticsModelProvider>();
else
    builder.Services.AddSingleton<IAnalyticsModelProvider, RuleBasedAnalyticsModelProvider>();
builder.Services.AddSingleton<AnalysisOrchestrator>();
builder.Services.AddSingleton<QueryHistoryRepository>();
builder.Services.AddSingleton<EvaluationService>();
builder.Services.AddSingleton<AnalysisMetrics>();
builder.Services.AddSingleton<SchemaDiscoveryService>();
var app = builder.Build();
app.UseExceptionHandler();
app.UseCors();
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("analytics");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", mode = "sql-server-reviewed-analytics" }));
app.Run();
public partial class Program;
