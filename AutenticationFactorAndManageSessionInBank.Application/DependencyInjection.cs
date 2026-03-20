using AutenticationFactorAndManageSessionInBank.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutenticationFactorAndManageSessionInBank.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<SessionQueryService>();
        return services;
    }
}
