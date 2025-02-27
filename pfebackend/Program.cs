using System.Net.Http.Headers;
using EmailService;
using pfebackend.Controllers;
using pfebackend.Extensions;
using pfebackend.Models.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerExplorer()
                .InjectDbContext(builder.Configuration)
                .AddAppConfig(builder.Configuration)
                .AddIdentityHandlersAndStores()
                .ConfigureIdentityOptions()
                .AddIdentityAuth(builder.Configuration);
EmailConfiguration emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailSender, EmailSender>();

var app = builder.Build();

app.ConfigureSwaggerExplorer()
   .ConfigureCORS(builder.Configuration)
   .AddIdentityAuthMiddlewares();

app.UseHttpsRedirection();

app.MapControllers();
app.MapGroup("/api")
   .MapIdentityApi<Consumer>();

app.MapGroup("/api")
   .MapIdentityConsumerEndpoints();



app.Run();







