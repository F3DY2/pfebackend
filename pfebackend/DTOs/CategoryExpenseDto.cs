namespace pfebackend.DTOs
{
    public class CategoryExpenseDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public float TotalAmount { get; set; }
        public float Percentage { get; set; }
    }
}
