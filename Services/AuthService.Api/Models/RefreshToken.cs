// <copyright file="RefreshToken.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuthService.Api.Models
{
    using System;

    /// <summary>
    /// Refresh token table model.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the token value.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time.
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// Gets a value indicating whether the token is expired.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= this.Expires;

        /// <summary>
        /// Gets or sets the revocation date and time (if revoked).
        /// </summary>
        public DateTime? Revoked { get; set; }

        /// <summary>
        /// Gets a value indicating whether the token is active.
        /// </summary>
        public bool IsActive => !this.IsExpired && this.Revoked == null;

        /// <summary>
        /// Gets or sets the associated user.
        /// </summary>
        public ApplicationUser User { get; set; }
    }
}