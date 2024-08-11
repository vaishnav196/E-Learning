namespace E_Learning.Models.Dashboard
{
    public class CombinedViewModel
    {
        public List<RegistrationData> RegistrationData { get; set; }
        public List<MonthlyAmountViewModel> MonthlyAmountData { get; set; }

        public int TotalRegisteredStudents { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
