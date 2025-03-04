using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using pfebackend.Config;
using pfebackend.Models.Database;
using pfebackend.Models.DataTransferObject;
using pfebackend.Models.DataTransferObjects;

namespace pfebackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityUserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IEmailSender _emailSender;

        public IdentityUserController(UserManager<User> userManager, IOptions<AppSettings> appSettings , IEmailSender emailSender)
        {
            _userManager = userManager;
            _appSettings = appSettings;
            _emailSender = emailSender;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> CreateUser([FromBody] UserRegistrationDto userRegistrationModel)
        {
            var user = new User
            {
                UserName = userRegistrationModel.Email,
                Email = userRegistrationModel.Email,
                first_Name = userRegistrationModel.first_Name,
                last_Name = userRegistrationModel.last_Name,
                PhoneNumber = userRegistrationModel.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, userRegistrationModel.Password);
            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result.Errors);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] LoginDto loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Value.JWTSecret));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("UserID", user.Id.ToString()) }),
                    Expires = DateTime.UtcNow.AddDays(10),
                    SigningCredentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);

                var userData = new
                {
                    user.first_Name,
                    user.last_Name,
                    user.PhoneNumber,
                    user.Email
                };
                return Ok(new { token , userData });
            }

            return BadRequest(new { message = "Username or password is incorrect." });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto userUpdateModel)
        {
            var user = await _userManager.FindByEmailAsync(userUpdateModel.Email);
            if (user == null)
                return NotFound("User not found");

            user.first_Name = userUpdateModel.first_Name;
            user.last_Name = userUpdateModel.last_Name;
            user.PhoneNumber = userUpdateModel.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User updated successfully");
        }
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
        {
            if(!ModelState.IsValid)
                return BadRequest("Invalid data");
            var user = await _userManager.FindByEmailAsync(forgotPassword.Email!);
            if(user is null)
                return BadRequest("User not found");
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var param = new Dictionary<string, string?>
            {
                {"token", token},
                {"email", forgotPassword.Email!}
            };

            var callback = QueryHelpers.AddQueryString(forgotPassword.ClientUri!, param);

            var message = new Message([user.Email], "Reset password token", callback);

             _emailSender.SendEmail(message);

            return Ok();
        }

       

    }
}
