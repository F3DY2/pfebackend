using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using pfebackend.Config;
using pfebackend.Models.Database;
using pfebackend.Models.DataTransferObject;

namespace pfebackend.Controllers
{
    public static class IdentityConsumerController
    {
        public static IEndpointRouteBuilder MapIdentityConsumerEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/signup", CreateUser);

            app.MapPost("/signin", SignIn);

            app.MapPut("/update", UpdateUser);
            return app;
        }

        private static async Task<IResult> CreateUser(UserManager<Consumer> UserManager,
                [FromBody] UserRegistrationDto userRegistrationModel)
        {
                Consumer user = new Consumer()
                {
                    UserName = userRegistrationModel.Email,
                    Email = userRegistrationModel.Email,
                    first_Name = userRegistrationModel.first_Name,
                    last_Name = userRegistrationModel.last_Name,
                    PhoneNumber = userRegistrationModel.PhoneNumber,
                };
                var result = await UserManager.CreateAsync(user,
                    userRegistrationModel.Password);
                if (result.Succeeded)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
        }

        private static async Task<IResult> SignIn(UserManager<Consumer> UserManager,
                [FromBody] LoginDto loginModel,
                IOptions<AppSettings> appSettings)
        {
            var user = await UserManager.FindByEmailAsync(loginModel.Email);
            if (user != null && await UserManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var signInKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(appSettings.Value.JWTSecret)
                                        );
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                new Claim("UserID",user.Id.ToString()),
                    }),
                    Expires = DateTime.UtcNow.AddDays(10),
                    SigningCredentials = new SigningCredentials(
                        signInKey,
                        SecurityAlgorithms.HmacSha256Signature
                        )
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Results.Ok(new { token });

            }
            else
                return Results.BadRequest(new { message = "Username or password is incorrect. " });
        }
        private static async Task<IResult> UpdateUser(
            UserManager<Consumer> userManager,
            [FromBody] UserUpdateDto userUpdateModel)
        {
            var user = await userManager.FindByEmailAsync(userUpdateModel.Email);

            if (user == null)
                return Results.NotFound("User not found");

            user.first_Name = userUpdateModel.first_Name;
            user.last_Name = userUpdateModel.last_Name;
            user.PhoneNumber = userUpdateModel.PhoneNumber;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return Results.BadRequest(result.Errors);
                
            if (!string.IsNullOrEmpty(userUpdateModel.OldPassword) && !string.IsNullOrEmpty(userUpdateModel.NewPassword))
            {
                var passwordChangeResult = await userManager.ChangePasswordAsync(user, userUpdateModel.OldPassword, userUpdateModel.NewPassword);
                if (!passwordChangeResult.Succeeded)
                    return Results.BadRequest(passwordChangeResult.Errors);
            }

            return Results.Ok("User updated successfully");
        
    }
    }
}
