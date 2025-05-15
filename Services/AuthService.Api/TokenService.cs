// <copyright file="TokenService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuthService.Api
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using AuthService.Api.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Service for handling JWT and refresh token operations.
    /// </summary>
    public class TokenService
    {
        private readonly IConfiguration config;
        private readonly AuthDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenService"/> class.
        /// </summary>
        /// <param name="config">The application configuration.</param>
        /// <param name="context">The database context.</param>
        /// <param name="userManager">User manager.</param>
        public TokenService(IConfiguration config, AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.config = config;
            this.context = context;
            this.userManager = userManager;
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user to generate token for.</param>
        /// <returns>The generated JWT token.</returns>
        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userRoles = await this.userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new("middle_name", user.MiddleName ?? string.Empty),
            };

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(this.config["Jwt:ExpireMinutes"]));

            var token = new JwtSecurityToken(
                this.config["Jwt:Issuer"],
                this.config["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates and stores a refresh token for the specified user.
        /// </summary>
        /// <param name="user">The user to generate token for.</param>
        /// <returns>The generated refresh token.</returns>
        public async Task<string> GenerateRefreshToken(ApplicationUser user)
        {
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                Revoked = null,
            };

            this.context.RefreshTokens.Add(refreshToken);
            await this.context.SaveChangesAsync();

            return refreshToken.Token;
        }

        /// <summary>
        /// Refreshes an expired JWT token using a valid refresh token.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <param name="refreshToken">The valid refresh token.</param>
        /// <returns>Authentication response with new tokens.</returns>
        /// <exception cref="SecurityTokenException">Thrown when tokens are invalid.</exception>
        public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = this.GetPrincipalFromExpiredToken(token);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await this.context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new SecurityTokenException("Invalid user");
            }

            var storedRefreshToken = await this.context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && x.UserId == userId);

            if (storedRefreshToken == null || storedRefreshToken.IsExpired || storedRefreshToken.Revoked != null)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            var newToken = await this.GenerateJwtToken(user);
            var newRefreshToken = await this.GenerateRefreshToken(user);

            storedRefreshToken.Revoked = DateTime.UtcNow;

            // Periodically remove old revoked/expired tokens
            var cutoff = DateTime.UtcNow.AddMonths(-1);
            var oldTokens = await this.context.RefreshTokens
                .Where(t => t.Revoked < cutoff || t.Expires < cutoff)
                .ToListAsync();

            this.context.RefreshTokens.RemoveRange(oldTokens);

            await this.context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
            };
        }

        /// <summary>
        /// Gets the principal from an expired token.
        /// </summary>
        /// <param name="token">The expired token.</param>
        /// <returns>The claims principal.</returns>
        /// <exception cref="SecurityTokenException">Thrown when token is invalid.</exception>
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config["Jwt:Key"])),
                ValidateLifetime = false,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}