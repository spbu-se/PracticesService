// <copyright file="20260704044613_AddedEmailField.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

#nullable disable

namespace CoreService.Api.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;
    using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

    /// <inheritdoc />
    public partial class AddedEmailField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "consultants",
                type: "text",
                defaultValue: string.Empty,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "lecturers",
                type: "text",
                defaultValue: string.Empty,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "students",
                type: "text",
                defaultValue: string.Empty,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "consultants");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "lecturers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "students");
        }
    }
}
