using FirstAPIProject.Application.Modules.Auth.Interfaces;
using FirstAPIProject.Application.Modules.Auth.Services;
using FirstAPIProject.Application.Modules.User.Interfaces;
using FirstAPIProject.Application.Modules.User.Services;
using FirstAPIProject.Application.Modules.Whitelist.Interfaces;
using FirstAPIProject.Application.Modules.Whitelist.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FirstAPIProject.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register all FluentValidation validators from Application assembly
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // Auth Services
            services.AddScoped<IAuthService, AuthService>();

            // User Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminUserService, AdminUserService>();

            // Whitelist Services
            services.AddScoped<IEmailWhitelistService, EmailWhitelistService>();

            return services;
        }
    }
}
