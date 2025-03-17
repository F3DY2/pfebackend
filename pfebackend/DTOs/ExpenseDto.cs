namespace pfebackend.DTOs
{
    public class ExpenseDto
    {
            public int? Id { get; set; }
            public string Name { get; set; }
            public Category Category { get; set; }
            public DateTime Date { get; set; }
            public float Amount { get; set; }
            public string? UserId { get; set; }
    }
    public enum Category
    {
        Food,
        Rent,
        Transportation,
        Health,
        Entertainment,
        Other
    }
}
