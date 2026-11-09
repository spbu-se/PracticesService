// <copyright file="InternalKeyAuthHandler.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace GatewayAuthHandler
{
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Encodings.Web;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Authenticates calls from other services of the system by the shared key in the X-Internal-Key header.
    /// The principal has no roles, so it never passes role based policies such as AdminOnly.
    /// </summary>
    public class InternalKeyAuthHandler : AuthenticationHandler<InternalKeyOptions>
    {
        /// <summary>
        /// Name of the header with the shared key.
        /// </summary>
        public const string HeaderName = "X-Internal-Key";

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalKeyAuthHandler"/> class.
        /// </summary>
        /// <param name="options">Authentication scheme options.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="encoder">Encoder.</param>
        /// <param name="systemClock">System clock.</param>
        public InternalKeyAuthHandler(
            IOptionsMonitor<InternalKeyOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock systemClock)
            : base(options, logger, encoder, systemClock)
        {
        }

        /// <summary>
        /// Checks whether the request carries the shared key.
        /// </summary>
        /// <param name="provided">Value of the X-Internal-Key header.</param>
        /// <param name="expected">The configured key.</param>
        /// <returns>True if the key is configured and equal to the provided one.</returns>
        public static bool IsValidKey(string? provided, string? expected)
        {
            if (string.IsNullOrEmpty(provided) || string.IsNullOrEmpty(expected))
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided), Encoding.UTF8.GetBytes(expected));
        }

        /// <inheritdoc/>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!this.Request.Headers.TryGetValue(HeaderName, out var provided))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!IsValidKey(provided.ToString(), this.Options.ApiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid internal key"));
            }

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "internal-service") }, "InternalKey");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), "InternalKey");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
