using System.Drawing.Design;

namespace pfebackend.DTOs
{
    public class ExpenseSumDto
    {
        public  string CategoryName { get; set; } 
        public DateTime Date { get; set; }
        public float Amount { get; set; }
    }
}
