using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using pfebackend.DTOs;

namespace pfebackend.Interfaces
{
    public interface IUserService
    {
        Task<IdentityResult> CreateUserAsync(UserRegistrationDto userRegistrationModel);
        Task<(string Token, object UserData)?> AuthenticateUser(LoginDto loginModel);
        Task<IdentityResult> EditUserProfile(UserUpdateDto userUpdateModel);
        Task<IdentityResult> ForgotPasswordHandler(ForgotPasswordDto forgotPassword);
        Task<IdentityResult> ResetPasswordHandler(ResetPasswordDto resetPassword);
    }
}
