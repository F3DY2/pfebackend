namespace pfebackend.DTOs
{
    public class DailyExpensesDto
    {
        public DateTime Date { get; set; }
        public float TotalExpensesForDate { get; set; }
        public List<CategoryExpenseDto> Categories { get; set; }

    }
}
