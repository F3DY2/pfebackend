using EmailService;

namespace pfebackend.Extensions
{
    public static class EmailExtensions
    {
        public static IServiceCollection InjectEmailService(
            this IServiceCollection services,
            IConfiguration config)
        {
            EmailConfiguration emailConfig = config.GetSection("EmailConfiguration")
                                                   .Get<EmailConfiguration>();

            services.AddSingleton(emailConfig);
            services.AddScoped<IEmailSender, EmailSender>();

            return services;
        }
    }
}
