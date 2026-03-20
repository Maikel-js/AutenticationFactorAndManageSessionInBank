using System.Security.Claims;
using System.Threading.RateLimiting;
using AutenticationFactorAndManageSessionInBank.Api.Contracts;
using AutenticationFactorAndManageSessionInBank.Api.Security;
using AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;
using AutenticationFactorAndManageSessionInBank.Application.Services;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Configuration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

AutenticationFactorAndManageSessionInBank.Application.DependencyInjection.AddApplication(builder.Services);
AutenticationFactorAndManageSessionInBank.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<CsrfTokenService>();
builder.Services.AddScoped<CookieTokenWriter>();
builder.Services.AddCors(options =>
{
    var security = builder.Configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>() ?? new SecurityOptions();
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(security.FrontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 8;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.AddSlidingWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.SegmentsPerWindow = 6;
        limiterOptions.PermitLimit = 120;
        limiterOptions.QueueLimit = 0;
    });
});

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
        if (exception is not null)
        {
            logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Unexpected server error",
            Detail = "The request could not be completed.",
            Status = StatusCodes.Status500InternalServerError
        });
    });
});

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<AutenticationFactorAndManageSessionInBank.Infrastructure.Seed.AuthDbSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self' https:; frame-ancestors 'none'; form-action 'self'; base-uri 'self'; object-src 'none'");
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    }

    await next();
});

app.Use(async (context, next) =>
{
    var methodsRequiringCsrf = new[] { HttpMethods.Post, HttpMethods.Put, HttpMethods.Patch, HttpMethods.Delete };
    if (methodsRequiringCsrf.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase))
    {
        var security = context.RequestServices.GetRequiredService<IOptions<SecurityOptions>>().Value;
        var csrfCookie = context.Request.Cookies[security.CsrfCookieName];
        var csrfHeader = context.Request.Headers["X-CSRF-TOKEN"].ToString();

        if (string.IsNullOrWhiteSpace(csrfCookie) || !string.Equals(csrfCookie, csrfHeader, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "CSRF validation failed",
                Detail = "The anti-forgery token is missing or invalid.",
                Status = StatusCodes.Status400BadRequest
            });
            return;
        }
    }

    await next();
});

app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

var auth = app.MapGroup("/api/auth")
    .WithTags("Authentication")
    .RequireRateLimiting("auth");

auth.MapGet("/csrf", (HttpContext httpContext, CsrfTokenService csrfTokenService, CookieTokenWriter cookieTokenWriter) =>
{
    var token = csrfTokenService.Generate();
    cookieTokenWriter.WriteCsrfCookie(httpContext, token);
    return Results.Ok(new { csrfToken = token });
})
.WithName("IssueCsrf")
.WithOpenApi();

auth.MapPost("/register", async (
    RegisterRequestDto request,
    AuthService authService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await authService.RegisterAsync(
            new RegisterRequest(
                request.Email,
                request.FullName,
                request.Password,
                request.TenantId,
                request.EnableMfa),
            cancellationToken);

        return Results.Created($"/api/users/{result.UserId}", result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new ProblemDetails { Title = "Registration failed", Detail = ex.Message });
    }
})
.WithName("Register")
.WithOpenApi();

auth.MapPost("/login", async (
    LoginRequestDto request,
    HttpContext httpContext,
    AuthService authService,
    CookieTokenWriter cookieTokenWriter,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await authService.LoginAsync(
            new LoginRequest(
                request.Email,
                request.Password,
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                httpContext.Request.Headers.UserAgent.ToString(),
                request.DeviceId,
                request.TenantId),
            cancellationToken);

        cookieTokenWriter.WriteAuthCookies(httpContext, result);
        return Results.Ok(ToAuthStatus(result));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new ProblemDetails { Title = "Authentication failed", Detail = ex.Message });
    }
})
.WithName("Login")
.WithOpenApi();

auth.MapPost("/mfa/verify", async (
    VerifyMfaRequestDto request,
    HttpContext httpContext,
    AuthService authService,
    CookieTokenWriter cookieTokenWriter,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await authService.VerifyMfaAsync(
            new VerifyMfaRequest(
                request.MfaTicket,
                request.OtpCode,
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
            cancellationToken);

        cookieTokenWriter.WriteAuthCookies(httpContext, result);
        return Results.Ok(ToAuthStatus(result));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new ProblemDetails { Title = "MFA verification failed", Detail = ex.Message });
    }
})
.WithName("VerifyMfa")
.WithOpenApi();

auth.MapPost("/refresh", async (
    HttpContext httpContext,
    AuthService authService,
    CookieTokenWriter cookieTokenWriter,
    IOptions<SecurityOptions> securityOptions,
    CancellationToken cancellationToken) =>
{
    try
    {
        var refreshToken = httpContext.Request.Cookies[securityOptions.Value.RefreshCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Results.BadRequest(new ProblemDetails { Title = "Token refresh failed", Detail = "Refresh cookie is missing." });
        }

        var result = await authService.RefreshAsync(
            new RefreshTokenRequest(
                refreshToken,
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
            cancellationToken);

        cookieTokenWriter.WriteAuthCookies(httpContext, result);
        return Results.Ok(ToAuthStatus(result));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new ProblemDetails { Title = "Token refresh failed", Detail = ex.Message });
    }
})
.WithName("RefreshToken")
.WithOpenApi();

auth.MapPost("/logout", (
    HttpContext httpContext,
    CookieTokenWriter cookieTokenWriter) =>
{
    cookieTokenWriter.ClearAuthCookies(httpContext);
    return Results.NoContent();
})
.WithName("Logout")
.WithOpenApi();

var me = app.MapGroup("/api/me")
    .RequireAuthorization()
    .RequireRateLimiting("api")
    .WithTags("Identity");

me.MapGet("/", async (
    HttpContext httpContext,
    AuthService authService,
    CancellationToken cancellationToken) =>
{
    var userId = GetRequiredUserId(httpContext.User);
    return Results.Ok(await authService.GetProfileAsync(userId, cancellationToken));
})
.WithName("GetCurrentUser")
.WithOpenApi();

var sessions = app.MapGroup("/api/sessions")
    .RequireAuthorization()
    .RequireRateLimiting("api")
    .WithTags("Sessions");

sessions.MapGet("/", async (
    HttpContext httpContext,
    SessionQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var userId = GetRequiredUserId(httpContext.User);
    return Results.Ok(await queryService.ListByUserIdAsync(userId, cancellationToken));
})
.WithName("ListSessions")
.WithOpenApi();

sessions.MapGet("/{sessionId:guid}", async (
    Guid sessionId,
    HttpContext httpContext,
    SessionQueryService queryService,
    CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await queryService.GetByIdAsync(GetRequiredUserId(httpContext.User), sessionId, cancellationToken));
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new ProblemDetails { Title = "Session not found", Detail = ex.Message });
    }
})
.WithName("GetSession")
.WithOpenApi();

sessions.MapDelete("/{sessionId:guid}", async (
    Guid sessionId,
    HttpContext httpContext,
    AuthService authService,
    CancellationToken cancellationToken) =>
{
    try
    {
        await authService.RevokeSessionAsync(GetRequiredUserId(httpContext.User), sessionId, cancellationToken);
        return Results.NoContent();
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new ProblemDetails { Title = "Session not found", Detail = ex.Message });
    }
})
.WithName("RevokeSession")
.WithOpenApi();

var security = app.MapGroup("/api/security")
    .RequireAuthorization()
    .RequireRateLimiting("api")
    .WithTags("Security");

security.MapGet("/notifications", async (
    HttpContext httpContext,
    SessionQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var userId = GetRequiredUserId(httpContext.User);
    return Results.Ok(await queryService.GetNotificationsAsync(userId, cancellationToken));
})
.WithName("GetSecurityNotifications")
.WithOpenApi();

security.MapGet("/devices", async (
    HttpContext httpContext,
    SessionQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var userId = GetRequiredUserId(httpContext.User);
    return Results.Ok(await queryService.GetDevicesAsync(userId, cancellationToken));
})
.WithName("GetDevices")
.WithOpenApi();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "fintech-auth-saas",
    utc = DateTimeOffset.UtcNow
}))
.RequireRateLimiting("api")
.WithName("Health")
.WithOpenApi();

app.Run();

static Guid GetRequiredUserId(ClaimsPrincipal principal)
{
    var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? principal.FindFirstValue("sub")
        ?? throw new InvalidOperationException("Subject claim is missing.");

    return Guid.Parse(subject);
}

static AuthStatusResponseDto ToAuthStatus(AuthResponse response) =>
    new(
        !response.RequiresMfa,
        response.RequiresMfa,
        response.SessionId,
        response.RiskScore,
        response.MfaTicket,
        response.MfaTicketExpiresAtUtc,
        response.User);
