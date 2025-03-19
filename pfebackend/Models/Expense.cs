using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace pfebackend.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Expense name is required.")]
        [StringLength(100, ErrorMessage = "Expense name cannot be longer than 100 characters.")]
        [Column(TypeName = "varchar(100)")] 
        public string Name { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public Category Category { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be Positive.")]
        public float Amount { get; set; }
        [Required]
        public string UserId { get; set; } 

        [ForeignKey("UserId")]
        public User User { get; set; } 
    }

    public enum Category
    {
        Food,
        Transport,
        Entertainment,
        Health,
        Electronics,
        Fashion,
        Housing,
        Others
    }
}
