// <copyright file="GatewayAuthHandler.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace GatewayAuthHandler
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Gateway authentication handler.
    /// </summary>
    public class GatewayAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayAuthHandler"/> class.
        /// </summary>
        /// <param name="options">Authenitcation scheme options.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="encoder">Encoder.</param>
        /// <param name="systemClock">System clock.</param>
        public GatewayAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock systemClock)
            : base(options, logger, encoder, systemClock)
        {
        }

        /// <summary>
        /// Handle authenticvation method.
        /// </summary>
        /// <returns>Authentication result.</returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!this.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail("No Authorization Header"));
            }

            var token = authHeader.ToString().Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var claims = jwtToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "GatewayAuth");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "GatewayAuth");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
