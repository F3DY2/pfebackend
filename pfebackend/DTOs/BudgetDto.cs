using pfebackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace pfebackend.DTOs
{
    public class BudgetDto
    {
        public int? Id { get; set; }

        public Category Category { get; set; }

        public float limitValue { get; set; }

        public float alertValue { get; set; }

        public string UserId { get; set; }

    }
}
