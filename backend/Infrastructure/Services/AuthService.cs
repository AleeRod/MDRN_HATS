using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MDRN_HATS.Application.Contracts;
using MDRN_HATS.Application.Exceptions;
using MDRN_HATS.Application.Interfaces;
using MDRN_HATS.Domain.Entities;
using MDRN_HATS.Infrastructure.Persistence;

namespace MDRN_HATS.Infrastructure.Services;

public sealed class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    AppDbContext dbContext,
    IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            throw new AppException("An account with this email already exists.", StatusCodes.Status409Conflict);
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            EmailConfirmed = false
        };
        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new AppException(
                string.Join(" ", createResult.Errors.Select(error => error.Description)),
                StatusCodes.Status400BadRequest);
        }

        var roleResult = await userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            throw new InvalidOperationException(
                $"Could not assign the Customer role: {string.Join(" ", roleResult.Errors.Select(error => error.Description))}");
        }

        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new AppException("Invalid email or password.", StatusCodes.Status401Unauthorized);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new AppException("Invalid email or password.", StatusCodes.Status401Unauthorized);
        }

        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var existingToken = await dbContext.RefreshTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        var now = DateTime.UtcNow;
        if (existingToken is null || existingToken.RevokedAtUtc is not null || existingToken.ExpiresAtUtc <= now)
        {
            throw new AppException("The refresh token is invalid or expired.", StatusCodes.Status401Unauthorized);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var revoked = await dbContext.RefreshTokens
            .Where(token => token.Id == existingToken.Id &&
                            token.RevokedAtUtc == null &&
                            token.ExpiresAtUtc > now)
            .ExecuteUpdateAsync(
                update => update.SetProperty(token => token.RevokedAtUtc, now),
                cancellationToken);
        if (revoked != 1)
        {
            throw new AppException("The refresh token is invalid or expired.", StatusCodes.Status401Unauthorized);
        }

        var user = await userManager.FindByIdAsync(existingToken.UserId.ToString());
        if (user is null)
        {
            throw new AppException("The refresh token is invalid or expired.", StatusCodes.Status401Unauthorized);
        }

        var response = await CreateSessionAsync(user, cancellationToken);
        var replacementTokenId = await dbContext.RefreshTokens
            .Where(token => token.TokenHash == HashToken(response.RefreshToken))
            .Select(token => token.Id)
            .SingleAsync(cancellationToken);
        var linked = await dbContext.RefreshTokens
            .Where(token => token.Id == existingToken.Id)
            .ExecuteUpdateAsync(
                update => update.SetProperty(token => token.ReplacedByTokenId, replacementTokenId),
                cancellationToken);
        if (linked != 1)
        {
            throw new InvalidOperationException("The rotated refresh token could not be linked.");
        }

        await transaction.CommitAsync(cancellationToken);
        return response;
    }

    private async Task<AuthResponse> CreateSessionAsync(User user, CancellationToken cancellationToken)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key must be configured through the environment.");
        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer must be configured through the environment.");
        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience must be configured through the environment.");
        var accessTokenMinutes = configuration.GetValue<int>("Jwt:AccessTokenMinutes", 15);
        var refreshTokenDays = configuration.GetValue<int>("Jwt:RefreshTokenDays", 14);
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(accessTokenMinutes);
        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(issuer, audience, claims, now, expiresAt, credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var rawRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawRefreshToken),
            ExpiresAtUtc = now.AddDays(refreshTokenDays)
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, rawRefreshToken, expiresAt, user.Email ?? string.Empty, roles.ToArray());
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
