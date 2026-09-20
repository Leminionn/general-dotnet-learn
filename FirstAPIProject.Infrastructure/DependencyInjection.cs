using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Application.Modules.Auth.Interfaces;
using FirstAPIProject.Application.Modules.User.Interfaces;
using FirstAPIProject.Application.Modules.Whitelist.Interfaces;
using FirstAPIProject.Infrastructure.Authentication;
using FirstAPIProject.Infrastructure.Persistence.Repositories;
using FirstAPIProject.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FirstAPIProject.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IEmailWhitelistRepository, EmailWhitelistRepository>();

            // Authentication & Security services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
