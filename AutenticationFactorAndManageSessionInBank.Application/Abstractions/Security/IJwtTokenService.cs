using AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;
using AutenticationFactorAndManageSessionInBank.Domain.Entities;

namespace AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;

public interface IJwtTokenService
{
    string CreateAccessToken(User user, Session session, DateTimeOffset expiresAtUtc);
    AccessTokenDescriptor Describe(User user, Session session, DateTimeOffset nowUtc);
}
