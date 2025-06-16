using Microsoft.AspNetCore.Mvc;

namespace InferenceLlvm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RerankerController : ControllerBase
    {
        private readonly IRerankerService _rerankerService;
        private readonly ILogger<RerankerController> _logger;

        public RerankerController(IRerankerService rerankerService, ILogger<RerankerController> logger)
        {
            _rerankerService = rerankerService;
            _logger = logger;
        }

        [HttpPost("rerank")]
        public async Task<ActionResult<RerankResponse>> Rerank([FromBody] RerankRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.VacancyTitle) || string.IsNullOrWhiteSpace(request.JobDescription))
                {
                    return BadRequest("Vacancy title and job description are required.");
                }

                _logger.LogInformation("Processing rerank request for vacancy: {VacancyTitle}", request.VacancyTitle);

                var response = await _rerankerService.RerankAsync(request);
                
                _logger.LogInformation("Rerank completed with probability: {Probability}", response.Probability);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing rerank request");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("health")]
        public async Task<ActionResult<HealthResponse>> Health()
        {
            try
            {
                var response = await _rerankerService.GetHealthAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking service health");
                return StatusCode(500, $"Service health check failed: {ex.Message}");
            }
        }
    }
}