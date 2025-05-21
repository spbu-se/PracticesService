// <copyright file="ThemesQueriesTests.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Tests;

using System;
using System.Linq;
using System.Threading.Tasks;
using CoreService.Api.Core;
using CoreService.Api.Core.Models;
using CoreService.Api.Core.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

/// <summary>
/// Unit tests for <see cref="ThemesQueries"/> using an in-memory database.
/// </summary>
[TestFixture]
public class ThemesQueriesTests
{
    private CoreContext dbContext;
    private ThemesQueries themesQueries;

    /// <summary>
    /// Sets up the in-memory database and initializes dependencies.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CoreContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.dbContext = new CoreContext(options);
        this.dbContext.Database.EnsureCreated();

        this.dbContext.Themes.Add(
            new Theme
        {
            Id = 1,
            Title = "Theme A",
            Description = "Test Description",
            Level = "Intermediate",
            Source = "Internal",
            Suggestedby = "Alice",
            Createddate = DateTime.UtcNow,
            Updateddate = DateTime.UtcNow,
        });

        this.dbContext.SaveChanges();
        this.themesQueries = new ThemesQueries(this.dbContext);
    }

    /// <summary>
    /// Disposes the database context after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        this.dbContext.Dispose();
    }

    /// <summary>
    /// Verifies that GetThemes returns all themes when no ID is specified.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetThemes_WithoutId_ReturnsAllThemes()
    {
        var result = await this.themesQueries.GetThemes();

        Assert.Multiple(
            () =>
        {
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Title, Is.EqualTo("Theme A"));
        });
    }

    /// <summary>
    /// Verifies that GetThemes returns the correct theme by ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetThemes_WithId_ReturnsMatchingTheme()
    {
        var result = await this.themesQueries.GetThemes(1);

        Assert.Multiple(
            () =>
        {
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Verifies that a new theme is inserted correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InsertTheme_AddsNewTheme()
    {
        var theme = new Theme
        {
            Title = "Theme B",
            Description = "New Desc",
            Level = "Beginner",
            Source = "External",
            Suggestedby = "Bob",
            Createddate = DateTime.UtcNow,
            Updateddate = DateTime.UtcNow,
        };

        var id = await this.themesQueries.InsertTheme(theme);

        Assert.Multiple(
            () =>
        {
            Assert.That(id, Is.GreaterThan(0));
            Assert.That(this.dbContext.Themes.Find(id), Is.Not.Null);
        });
    }

    /// <summary>
    /// Verifies that updating an existing theme returns Ok and persists changes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateTheme_ValidTheme_ReturnsOk()
    {
        var updated = new Theme
        {
            Id = 1,
            Title = "Updated Title",
            Description = "Updated Desc",
            Level = "Advanced",
            Source = "Updated Source",
            Suggestedby = "Charlie",
            Updateddate = DateTime.UtcNow,
        };

        var result = await this.themesQueries.UpdateTheme(updated);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.Multiple(
            () =>
        {
            Assert.That(result.ToString(), Does.Contain("Ok"));
            Assert.That(this.dbContext.Themes.Find(1)?.Title, Is.EqualTo("Updated Title"));
        });
    }

    /// <summary>
    /// Verifies that updating a non-existent theme returns BadRequest.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateTheme_NonExistentTheme_ReturnsBadRequest()
    {
        var theme = new Theme
        {
            Id = 999,
            Title = "Ghost Theme",
        };

        var result = await this.themesQueries.UpdateTheme(theme);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.That(result.ToString(), Does.Contain("BadRequest"));
    }

    /// <summary>
    /// Verifies that a theme can be deleted and returns Ok.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteTheme_ExistingId_ReturnsOkAndRemovesTheme()
    {
        var result = await this.themesQueries.DeleteTheme(1);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.Multiple(
            () =>
        {
            Assert.That(result.ToString(), Does.Contain("Ok"));
            Assert.That(this.dbContext.Themes.Find(1), Is.Null);
        });
    }
}
