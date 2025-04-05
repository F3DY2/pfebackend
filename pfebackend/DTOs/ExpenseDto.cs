using pfebackend.Models;

namespace pfebackend.DTOs
{
    public class ExpenseDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public float Amount { get; set; }
        public string? UserId { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

    }

}
