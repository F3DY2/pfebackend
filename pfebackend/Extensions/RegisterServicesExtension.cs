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
                    .AddScoped<IBudgetPeriodService, BudgetPeriodService>()
                    .AddScoped<IBudgetService, BudgetService>()
                    .AddScoped<INotificationService, NotificationService>()
                    .AddScoped<IDashboardService, DashboardService>()
                    .AddScoped<ICategoryService, CategoryService>()
                    .AddScoped<IPredictedMonthlyExpenseService, PredictedMonthlyExpenseService>();
            return services;
        }
    }
}
