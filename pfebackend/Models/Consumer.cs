using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace pfebackend.Models
{
    public class Consumer:IdentityUser
    {
        [PersonalData]
        [Column(TypeName ="nvarchar(100)")]
        public string first_Name { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string last_Name { get; set; }



    }
}
