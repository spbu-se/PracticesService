// <copyright file="InternalKeyOptions.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace GatewayAuthHandler
{
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// Options of the internal service key authentication.
    /// </summary>
    public class InternalKeyOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Gets or sets the shared key. Requests without it are never authenticated by this scheme.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
    }
}
