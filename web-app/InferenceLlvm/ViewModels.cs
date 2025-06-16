using System.ComponentModel.DataAnnotations;

namespace InferenceLlvm
{
    public class RerankViewModel
    {
        [Required(ErrorMessage = "Vacancy title is required")]
        [Display(Name = "Vacancy Title")]
        public string VacancyTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job description is required")]
        [Display(Name = "Job Description")]
        public string JobDescription { get; set; } = string.Empty;

        [Display(Name = "Custom Instruction (Optional)")]
        public string Instruction { get; set; } = "Given a vacancy title, retrieve relevant job description of the candidate that is suitable for the vacancy";

        public bool HasResult { get; set; }
        public RerankResult? Result { get; set; }
    }

    public class RerankResult
    {
        public double Probability { get; set; }
        public double Score { get; set; }
        public bool IsMatch { get; set; }
        
        public string ProbabilityPercentage => (Probability * 100).ToString("F2");
        public string MatchStatus => IsMatch ? "Good Match" : "Poor Match";
        public string MatchClass => IsMatch ? "text-success" : "text-danger";
    }
}