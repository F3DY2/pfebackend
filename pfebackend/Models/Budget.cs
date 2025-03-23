using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pfebackend.Models
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Category Category { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be Positive.")]
        public float LimitValue { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be Positive.")]
        public float AlertValue { get; set; }
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

    }
}
