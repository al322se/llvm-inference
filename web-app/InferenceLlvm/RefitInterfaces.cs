using Refit;

namespace InferenceLlvm
{
    public interface IRerankerService
    {
        [Post("/rerank")]
        Task<RerankResponse> RerankAsync([Body] RerankRequest request);

        [Get("/health")]
        Task<HealthResponse> GetHealthAsync();
    }

    public class RerankRequest
    {
        public string VacancyTitle { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string Instruction { get; set; } = "Given a vacancy title, retrieve relevant job description of the candidate that is suitable for the vacancy";
    }

    public class RerankResponse
    {
        public double Probability { get; set; }
        public double Score { get; set; }
    }

    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public bool ModelLoaded { get; set; }
    }
}