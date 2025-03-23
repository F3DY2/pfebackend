using pfebackend.Interfaces;
using pfebackend.Services;

namespace pfebackend.Extensions
{
    public static class RegisterServicesExtension
    {
        public static IServiceCollection InjectServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>()
                    .AddScoped<IExpenseService, ExpenseService>()
                    .AddScoped<ICsvImportService, CsvImportService>()
                    .AddScoped<IBudgetService, BudgetService>();
            return services;
        }
    }
}
