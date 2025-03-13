using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using pfebackend.Config;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class IdentityUserController : ControllerBase
    {

        private readonly IUserService _userService;

        public IdentityUserController(IUserService userService)
        {
            _userService = userService; 
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> CreateUser([FromBody] UserRegistrationDto userRegistrationModel)
        {
            IdentityResult result = await _userService.CreateUserAsync(userRegistrationModel);
            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result.Errors);
        }
        
        [AllowAnonymous]
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] LoginDto loginModel)
        {
            (string Token, object UserData)? result = await _userService.AuthenticateUser(loginModel);
            if (result != null)
            {
                return Ok(new { token = result.Value.Token, userData = result.Value.UserData });
            }
            return BadRequest(new { message = "Username or password is incorrect." });

        }
        
       
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto userUpdateModel)
        {
            IdentityResult result = await _userService.EditUserProfile(userUpdateModel);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User updated successfully" });
        }

        [AllowAnonymous]
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            IdentityResult result = await _userService.ForgotPasswordHandler(forgotPassword);

            if (result.Succeeded)
                return Ok(new { message = "Email sent" });

            return BadRequest(result.Errors);
        }
        
        
        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data" });

            IdentityResult result = await _userService.ResetPasswordHandler(resetPassword);

            if (result.Succeeded)
            {
                // Renvoyer un message de succès sous forme de JSON
                return Ok(new { message = "Password reset successfully" });
            }

            // Renvoyer les erreurs sous forme de JSON
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }


    }
}
