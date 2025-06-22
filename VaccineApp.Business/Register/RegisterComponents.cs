using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VaccineApp.Business.Interfaces;
using VaccineApp.Business.Services;
using VaccineApp.Data.Entities;

namespace VaccineApp.Business.Register
{
    public static class RegisterComponents
    {
        public static void AddComponents(this IServiceCollection services)
        {
            #region Main Module Services
            services.AddTransient<IFreezerStockService, FreezerStockService>();
            //services.AddTransient<IFreezerService, FreezerService>();
            services.AddTransient<IFreezerTemperatureService, FreezerTemperatureService>();
            //services.AddTransient<IVaccineService, VaccineService>();
            //services.AddTransient<IVaccineFreezerService, VaccineFreezerService>();
            //services.AddTransient<IVaccineOrderService, VaccineFreezerService>();
            services.AddTransient<IFreezerStockService, FreezerStockService>();
            services.AddTransient<IFreezerStockService, FreezerStockService>();
            services.AddTransient<IFreezerStockService, FreezerStockService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IOutboxMessageService, OutboxMessageService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            #endregion
        }
    }
}
