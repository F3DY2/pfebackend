using pfebackend.Data;
using pfebackend.Extensions;
using pfebackend.Hubs;
using pfebackend.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Swagger
builder.Services.AddSwaggerExplorer();

// Database & Configuration
builder.Services.InjectDbContext(builder.Configuration);
builder.Services.AddAppConfig(builder.Configuration);

// Identity & Authentication
builder.Services.AddIdentityHandlersAndStores();
builder.Services.ConfigureIdentityOptions();
builder.Services.AddIdentityAuth(builder.Configuration);

// Services
builder.Services.InjectServices();
builder.Services.InjectEmailService(builder.Configuration);
// Add after builder.Services
builder.Services.AddSignalR();

var app = builder.Build();

app.ConfigureSwaggerExplorer()
   .ConfigureCORS(builder.Configuration)
   .AddIdentityAuthMiddlewares();

app.UseHttpsRedirection();
// Add before app.MapControllers()
app.MapHub<NotificationHub>("/notificationHub");
app.MapControllers();

app.MapGroup("/api")
   .MapIdentityApi<User>();


app.Run();







