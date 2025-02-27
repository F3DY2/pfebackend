using Microsoft.EntityFrameworkCore;
using pfebackend.Data;

namespace pfebackend.Extensions
{
    public static class EFCoreExtensions
    {
        public static IServiceCollection InjectDbContext(
            this IServiceCollection services,
            IConfiguration config)
        {

            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DevDB")));
            return services;
        }
    }
}
