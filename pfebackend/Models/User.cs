using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace pfebackend.Models
{
    public class User : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string FirstName { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string LastName { get; set; }

        public string? Avatar { get; set; }

        public string? AgriculturalHouseHoldIndicator { get; set; }

        public int? TotalNumberOfFamilyMembers { get; set; }
        public int? TotalNumberOfFamilyMembersEmployed { get; set; }
    }
}
