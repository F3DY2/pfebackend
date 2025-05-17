using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmailService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using pfebackend.Config;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPredictedMonthlyExpenseService _predictedMonthlyExpenseService;


        public UserService(UserManager<User> userManager, IOptions<AppSettings> appSettings, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor, IPredictedMonthlyExpenseService predictedMonthlyExpenseService)
        {
            _userManager = userManager;
            _appSettings = appSettings;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _predictedMonthlyExpenseService = predictedMonthlyExpenseService;
        }

        public async Task<IdentityResult> CreateUserAsync(UserRegistrationDto userRegistrationModel)
        {
            User user = new User
            {
                UserName = userRegistrationModel.Email,
                Email = userRegistrationModel.Email,
                FirstName = userRegistrationModel.FirstName,
                LastName = userRegistrationModel.LastName,
                PhoneNumber = userRegistrationModel.PhoneNumber,
                Avatar = userRegistrationModel.Avatar,
                TotalNumberOfBedrooms = userRegistrationModel.TotalNumberOfBedrooms,
                TotalNumberOfCars = userRegistrationModel.TotalNumberOfCars,
                TotalNumberOfFamilyMembers = userRegistrationModel.TotalNumberOfFamilyMembers,
                TotalNumberOfFamilyMembersEmployed = userRegistrationModel.TotalNumberOfFamilyMembersEmployed
            };

            return await _userManager.CreateAsync(user, userRegistrationModel.Password);
        }
        public async Task<(string Token, object UserData)?> AuthenticateUser(LoginDto loginModel)
        {
            User user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                SymmetricSecurityKey signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Value.JWTSecret));
                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("UserID", user.Id.ToString()) }),
                    Expires = DateTime.UtcNow.AddDays(10),
                    SigningCredentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256Signature)
                };

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
                string token = tokenHandler.WriteToken(securityToken);

                object userData = new
                {
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    user.Email,
                    user.Avatar,
                    user.TotalNumberOfBedrooms,
                    user.TotalNumberOfCars,
                    user.TotalNumberOfFamilyMembers,
                    user.TotalNumberOfFamilyMembersEmployed,
                };
                return ( token, userData );
            }
            return null;
        }
        public async Task<IdentityResult> EditUserProfile(UserUpdateDto userUpdateModel)
        {
            User user = await _userManager.FindByEmailAsync(userUpdateModel.Email);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            

            user.FirstName = userUpdateModel.FirstName;
            user.LastName = userUpdateModel.LastName;
            user.PhoneNumber = userUpdateModel.PhoneNumber;
            user.Avatar = userUpdateModel.Avatar;
            user.TotalNumberOfBedrooms =userUpdateModel.TotalNumberOfBedrooms;
            user.TotalNumberOfCars = userUpdateModel.TotalNumberOfCars;
            user.TotalNumberOfFamilyMembers = userUpdateModel.TotalNumberOfFamilyMembers;
            user.TotalNumberOfFamilyMembersEmployed = userUpdateModel.TotalNumberOfFamilyMembersEmployed;
            var updateResult = await _userManager.UpdateAsync(user);

            // Après la mise à jour, appeler la méthode de recalcul
            if (updateResult.Succeeded)
            {
                try
                {
                    await _predictedMonthlyExpenseService.updatePredictedExpenseWhenUserDetailsChanged(user.Id);
                }
                catch (Exception ex)
                {
                    // Optional: log the exception
                    Console.WriteLine($"Prediction service failed: {ex.Message}");
                    // or use a logger like _logger.LogError(ex, "Prediction update failed");
                }
            }

            return updateResult;
        }
        public async Task<IdentityResult> ForgotPasswordHandler(ForgotPasswordDto forgotPassword)
        {

            User user = await _userManager.FindByEmailAsync(forgotPassword.Email!);

            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            Dictionary<string, string?> param = new Dictionary<string, string?>
            {
                {"token", token},
                {"email", forgotPassword.Email!}
            };

            string callback = QueryHelpers.AddQueryString(forgotPassword.ClientUri!, param);

            Message message = new Message([user.Email], "Reset password token", callback);

            _emailSender.SendEmail(message);

            return IdentityResult.Success;
        }
        public async Task<IdentityResult> ResetPasswordHandler(ResetPasswordDto resetPassword)
        {
            User user = await _userManager.FindByEmailAsync(resetPassword.Email!);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            if (!string.IsNullOrEmpty(resetPassword.CurrentPassword) && !string.IsNullOrEmpty(resetPassword.NewPassword))
            {
                return await _userManager.ChangePasswordAsync(user, resetPassword.CurrentPassword, resetPassword.NewPassword);
            }
            return await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.NewPassword);
        }

        public string GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in the token.");
            }

            return userIdClaim;
        }
    }

}
