using System.Net.Http.Headers;
using EmailService;
using pfebackend.Controllers;
using pfebackend.Extensions;
using pfebackend.Interfaces;
using pfebackend.Models;
using pfebackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerExplorer()
                .InjectDbContext(builder.Configuration)
                .AddAppConfig(builder.Configuration)
                .AddIdentityHandlersAndStores()
                .ConfigureIdentityOptions()
                .AddIdentityAuth(builder.Configuration)
                .AddScoped<IUserService, UserService>()
                .AddScoped<IExpenseService, ExpenseService>()
                .AddScoped<ICsvImportService, CsvImportService>();
builder.Services.AddHttpContextAccessor();
builder.Services.InjectEmailService(builder.Configuration);

var app = builder.Build();

app.ConfigureSwaggerExplorer()
   .ConfigureCORS(builder.Configuration)
   .AddIdentityAuthMiddlewares();

app.UseHttpsRedirection();

app.MapControllers();

app.MapGroup("/api")
   .MapIdentityApi<User>();

app.Run();







