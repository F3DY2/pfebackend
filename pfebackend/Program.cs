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







