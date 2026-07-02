// <copyright file="ThemesQueriesTests.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Tests;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using CoreService.Api.Core;
using CoreService.Api.Core.Models;
using CoreService.Api.Core.Queries;
using CoreService.Api.Services;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

/// <summary>
/// Unit tests for <see cref="ThemesQueries"/> using an in-memory database.
/// </summary>
[TestFixture]
public class ThemesQueriesTests
{
    private CoreContext dbContext;
    private ThemesQueries themesQueries;
    private Mock<IPublishEndpoint> mockPublishEndpoint;
    private Mock<UserResolverService> mockUserResolver;
    private Mock<ILogger<ThemesQueries>> mockLogger;

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
                Suggestedby = "test-user-id",
                Createddate = DateTime.UtcNow,
                Updateddate = DateTime.UtcNow,
            });

        this.dbContext.SaveChanges();

        this.mockPublishEndpoint = new Mock<IPublishEndpoint>();
        this.mockUserResolver = new Mock<UserResolverService>(MockBehavior.Default, new Mock<IHttpClientFactory>().Object);
        this.mockLogger = new Mock<ILogger<ThemesQueries>>();

        this.themesQueries = new ThemesQueries(
            this.dbContext,
            this.mockPublishEndpoint.Object,
            this.mockUserResolver.Object,
            this.mockLogger.Object);
    }

    /// <summary>
    /// Cleans up the in-memory database after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        this.dbContext.Database.EnsureDeleted();
        this.dbContext.Dispose();
    }

    /// <summary>
    /// Tests that GetThemes returns all themes when no id is provided.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetThemes_WhenNoIdProvided_ReturnsAllThemes()
    {
        this.dbContext.Themes.Add(
            new Theme
            {
                Id = 2,
                Title = "Theme B",
                Description = "Another Test Description",
                Level = "Beginner",
                Source = "External",
                Suggestedby = "another-user",
                Createddate = DateTime.UtcNow,
                Updateddate = DateTime.UtcNow,
            });
        await this.dbContext.SaveChangesAsync();

        var result = await this.themesQueries.GetThemes();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    /// <summary>
    /// Tests that GetThemes returns a specific theme when an id is provided.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetThemes_WhenIdProvided_ReturnsSpecificTheme()
    {
        var result = await this.themesQueries.GetThemes(1);

        var enumerable = result as Theme[] ?? result.ToArray();
        Assert.That(enumerable.Count(), Is.EqualTo(1));
        Assert.That(enumerable.First().Id, Is.EqualTo(1));
        Assert.That(enumerable.First().Title, Is.EqualTo("Theme A"));
    }

    /// <summary>
    /// Tests that InsertTheme adds a new theme to the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InsertTheme_WhenCalled_AddsThemeToDatabase()
    {
        var newTheme = new Theme
        {
            Title = "New Theme",
            Description = "New Description",
            Level = "Advanced",
            Source = "Internal",
            Suggestedby = "new-user",
            Createddate = DateTime.UtcNow,
            Updateddate = DateTime.UtcNow,
        };

        var id = await this.themesQueries.InsertTheme(newTheme);

        Assert.That(id, Is.GreaterThan(0));
        var savedTheme = await this.dbContext.Themes.FindAsync(id);
        Assert.That(savedTheme, Is.Not.Null);
        Assert.That(savedTheme.Title, Is.EqualTo("New Theme"));
    }

    /// <summary>
    /// Tests that UpdateTheme updates an existing theme.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateTheme_WhenCalled_UpdatesTheme()
    {
        var themeToUpdate = new Theme
        {
            Id = 1,
            Title = "Updated Title",
            Description = "Updated Description",
            Level = "Expert",
            Source = "External",
            Suggestedby = "updated-user",
            Isarchived = true,
        };

        var result = await this.themesQueries.UpdateTheme(themeToUpdate);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.That(result, Is.InstanceOf<Ok>());

        var updatedTheme = await this.dbContext.Themes.FindAsync(1);
        Assert.That(updatedTheme, Is.Not.Null);
        Assert.That(updatedTheme.Title, Is.EqualTo("Updated Title"));
        Assert.That(updatedTheme.Description, Is.EqualTo("Updated Description"));
        Assert.That(updatedTheme.Level, Is.EqualTo("Expert"));
        Assert.That(updatedTheme.Isarchived, Is.True);
    }

    /// <summary>
    /// Tests that UpdateTheme returns BadRequest when theme is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateTheme_WhenThemeNotFound_ReturnsBadRequest()
    {
        var themeToUpdate = new Theme
        {
            Id = 999,
            Title = "Non-existent Theme",
        };

        var result = await this.themesQueries.UpdateTheme(themeToUpdate);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.That(result, Is.InstanceOf<BadRequest>());
    }

    /// <summary>
    /// Tests that DeleteTheme removes a theme from the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteTheme_WhenCalled_RemovesThemeFromDatabase()
    {
        var result = await this.themesQueries.DeleteTheme(1);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.That(result, Is.InstanceOf<Ok>());

        var deletedTheme = await this.dbContext.Themes.FindAsync(1);
        Assert.That(deletedTheme, Is.Null);
    }

    /// <summary>
    /// Tests that DeleteTheme throws InvalidOperationException when theme not found.
    /// </summary>
    [Test]
    public void DeleteTheme_WhenThemeNotFound_ThrowsInvalidOperationException()
    {
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await this.themesQueries.DeleteTheme(999));
    }
}