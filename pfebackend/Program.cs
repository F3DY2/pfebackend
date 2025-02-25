using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pfebackend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddIdentityApiEndpoints<Consumer>().AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;



});
builder.Services.AddDbContext<AppDbContext>(options=>
options.UseSqlServer(builder.Configuration.GetConnectionString("DevDB")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGroup("/api")
    .MapIdentityApi<Consumer>();

app.MapPost("/api/signup", async (UserManager<Consumer> UserManager,
    [FromBody] UserRegistrationModel userRegistrationModel) =>
{
    Consumer user = new Consumer()
    {
        UserName = userRegistrationModel.Email,
        Email = userRegistrationModel.Email,
        first_Name= userRegistrationModel.first_Name,
        last_Name = userRegistrationModel.last_Name,
        PhoneNumber= userRegistrationModel.PhoneNumber,
    };
    var result =await UserManager.CreateAsync(user,
        userRegistrationModel.Password);
    if (result.Succeeded)
        return Results.Ok(result);
    else
        return Results.BadRequest(result);
}
    );

app.Run();

public class UserRegistrationModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string first_Name { get; set; }

    public string last_Name { get; set; }

    public string PhoneNumber { get; set; }

}







