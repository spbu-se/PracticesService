// <copyright file="LoginModel.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuthService.Api.Models
{
    /// <summary>
    /// Login model.
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Gets or sets user email.
        /// </summary>
        public required string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user password.
        /// </summary>
        public required string Password { get; set; } = string.Empty;
    }
}
