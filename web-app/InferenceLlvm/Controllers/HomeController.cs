using Microsoft.AspNetCore.Mvc;

namespace InferenceLlvm.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRerankerService _rerankerService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IRerankerService rerankerService, ILogger<HomeController> logger)
        {
            _rerankerService = rerankerService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new RerankViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(RerankViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new RerankRequest
                    {
                        VacancyTitle = model.VacancyTitle,
                        JobDescription = model.JobDescription,
                        Instruction = model.Instruction
                    };

                    var response = await _rerankerService.RerankAsync(request);
                    
                    model.Result = new RerankResult
                    {
                        Probability = response.Probability,
                        Score = response.Score,
                        IsMatch = response.Probability > 0.5
                    };

                    model.HasResult = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing rerank request");
                    ModelState.AddModelError("", $"Error processing request: {ex.Message}");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Health()
        {
            try
            {
                var health = await _rerankerService.GetHealthAsync();
                return Json(health);
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = ex.Message });
            }
        }
    }
}