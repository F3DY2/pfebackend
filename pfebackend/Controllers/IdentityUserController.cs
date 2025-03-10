using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmailService;
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
    [Route("[controller]")]
    public class IdentityUserController : ControllerBase
    {

        private readonly IUserService _userService;

        public IdentityUserController(IUserService userService)
        {
            _userService = userService; 
        }

        [HttpPost("signup")]
        public async Task<IActionResult> CreateUser([FromBody] UserRegistrationDto userRegistrationModel)
        {
            IdentityResult result = await _userService.CreateUserAsync(userRegistrationModel);
            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result.Errors);
        }

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

            return Ok("User updated successfully");
        }
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            IdentityResult result = await _userService.ForgotPasswordHandler(forgotPassword);

            if (result.Succeeded)
                return Ok("Email sent");

            return BadRequest(result.Errors);
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            IdentityResult result = await _userService.ResetPasswordHandler(resetPassword);

            if (result.Succeeded)
            {
                return Ok("Password reset successfully");
            }
            return BadRequest(result.Errors);
        }

    }
}
