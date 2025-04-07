namespace pfebackend.DTOs
{
    public class DashboardDataDto
    {
        public float TotalBudget { get; set; }
        public float TotalExpenses { get; set; }
        public float BudgetLeft { get; set; }
        public List<CategoryExpenseDto> ExpensesByCategory { get; set; }
        public List<ExpenseDto> RecentExpenses { get; set; }
        public BudgetPeriodDto BudgetPeriod { get; set; }
        public string Message { get; set; }
    }
}
