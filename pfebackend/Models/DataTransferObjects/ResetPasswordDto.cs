using System.ComponentModel.DataAnnotations;

namespace pfebackend.Models.DataTransferObjects
{
    public class ResetPasswordDto
    {
        public string? CurrentPassword { get; set; }
        [Required(ErrorMessage ="Password is required")]
        public string NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage ="The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }
        public string? Token { get; set; }
    }
}
