using System.Text;
using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Persistence;
using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Configuration;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Persistence;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Repositories;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Security;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));

        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("AuthDb") ?? "Data Source=auth.db"));

        services.AddScoped<IAuthDbContext>(provider => provider.GetRequiredService<AuthDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<PasswordHasher>();
        services.AddSingleton<IRefreshTokenFactory, RefreshTokenFactory>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IRiskScoringService, RiskScoringService>();
        services.AddSingleton<ITotpService, TotpService>();
        services.AddSingleton<TotpService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<AuthDbSeeder>();
        services.AddHttpContextAccessor();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var securityOptions = configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>() ?? new SecurityOptions();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(securityOptions.AccessCookieName, out var cookieToken))
                        {
                            context.Token = cookieToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}
