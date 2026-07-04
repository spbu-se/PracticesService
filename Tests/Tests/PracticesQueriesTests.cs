// <copyright file="PracticesQueriesTests.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Tests;

using System;
using System.Linq;
using System.Threading.Tasks;
using CoreService.Api.Core;
using CoreService.Api.Core.Models;
using CoreService.Api.Core.Queries;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

/// <summary>
/// Unit tests for <see cref="PracticesQueries"/> using in-memory database.
/// </summary>
[TestFixture]
public class PracticesQueriesTests
{
    private CoreContext dbContext;
    private PracticesQueries practicesQueries;
    private Mock<IPublishEndpoint> mockPublishEndpoint;
    private Mock<ILogger<PracticesQueries>> mockLogger;

    /// <summary>
    /// Sets up in-memory database and initializes dependencies.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CoreContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.dbContext = new CoreContext(options);
        this.dbContext.Database.EnsureCreated();

        this.mockPublishEndpoint = new Mock<IPublishEndpoint>();
        this.mockLogger = new Mock<ILogger<PracticesQueries>>();

        // Seed data
        var student = new Student
        {
            Id = 1,
            Userid = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
        };

        this.dbContext.Students.Add(student);

        var supervisor = new Lecturer
        {
            Id = 1,
            Userid = "user-2",
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
        };

        this.dbContext.Lecturers.Add(supervisor);

        var consultant = new Consultant
        {
            Id = 1,
            Userid = "user-3",
            FirstName = "Bob",
            LastName = "Johnson",
            Contact = "Contact",
        };

        this.dbContext.Consultants.Add(consultant);

        var theme = new Theme
        {
            Id = 1,
            Title = "Theme A",
            Description = "New Desc",
            Level = "Beginner",
            Source = "External",
            Suggestedby = "Bob",
            Createddate = DateTime.UtcNow,
            Updateddate = DateTime.UtcNow,
        };

        this.dbContext.Themes.Add(theme);

        var practice = new Practice
        {
            Id = 1,
            Studentid = 1,
            Student = student,
            Status = "Pending",
            Finalgrade = null,
            Updateddate = DateTime.UtcNow,
            Type = "TypeA",
            Consultantid = 1,
            Supervisorid = 1,
            Themeid = 1,
            Supervisor = supervisor,
            Theme = theme,
        };

        this.dbContext.Practices.Add(practice);
        this.dbContext.SaveChanges();

        this.practicesQueries = new PracticesQueries(
            this.dbContext,
            this.mockPublishEndpoint.Object,
            this.mockLogger.Object);
    }

    /// <summary>
    /// Disposes the database context.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        this.dbContext.Database.EnsureDeleted();
        this.dbContext.Dispose();
    }

    /// <summary>
    /// Verifies that all practices are returned when no ID is specified.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetPractices_WithoutId_ReturnsAllPractices()
    {
        var result = await this.practicesQueries.GetPractices();

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Status, Is.EqualTo("Pending"));
            });
    }

    /// <summary>
    /// Verifies that a practice is returned by its ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetPractices_WithId_ReturnsMatchingPractice()
    {
        var result = await this.practicesQueries.GetPractices(1);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Id, Is.EqualTo(1));
            });
    }

    /// <summary>
    /// Verifies that queried practices are returned for a valid userId.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetPracticesByStudent_WithValidUserId_ReturnsPractices()
    {
        var result = await this.practicesQueries.GetPracticesByStudent("user-1");

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Student.Userid, Is.EqualTo("user-1"));
            });
    }

    /// <summary>
    /// Verifies that an empty list is returned if userId is empty.
    /// </summary>
    /// <param name="userId">User Id.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestCase("")]
    public async Task GetPracticesByStudent_WithEmptyUserId_ReturnsEmptyList(string userId)
    {
        var result = await this.practicesQueries.GetPracticesByStudent(userId);

        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Verifies that practices by supervisor are returned for a valid supervisor userId.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetPracticesBySupervisor_WithValidUserId_ReturnsPractices()
    {
        var result = await this.practicesQueries.GetPracticesBySupervisor("user-2");

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Supervisor.Userid, Is.EqualTo("user-2"));
            });
    }

    /// <summary>
    /// Verifies that an empty list is returned if supervisor userId is empty.
    /// </summary>
    /// <param name="userId">User Id.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestCase("")]
    public async Task GetPracticesBySupervisor_WithEmptyUserId_ReturnsEmptyList(string userId)
    {
        var result = await this.practicesQueries.GetPracticesBySupervisor(userId);

        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Verifies that a new practice can be inserted.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InsertPractice_AddsNewPractice()
    {
        var practice = new Practice
        {
            Studentid = 1,
            Status = "New",
            Finalgrade = "B",
            Type = "TypeB",
        };

        var id = await this.practicesQueries.InsertPractice(practice);

        Assert.Multiple(
            () =>
            {
                Assert.That(id, Is.GreaterThan(0));
                Assert.That(this.dbContext.Practices.Find(id), Is.Not.Null);
            });
    }

    /// <summary>
    /// Verifies that updating an existing practice succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdatePractice_ValidPractice_ReturnsOk()
    {
        var updatedPractice = new Practice
        {
            Id = 1,
            Finalgrade = "A",
            Type = "TypeC",
        };

        var result = await this.practicesQueries.UpdatePractice(updatedPractice);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.That(result, Is.InstanceOf<Ok>());

        // Clear change tracker to force fresh load
        this.dbContext.ChangeTracker.Clear();

        var practiceInDb = await this.dbContext.Practices
            .FirstOrDefaultAsync(p => p.Id == 1);

        Assert.That(practiceInDb, Is.Not.Null);
        Assert.Multiple(
            () =>
            {
                Assert.That(practiceInDb?.Status, Is.EqualTo("Завершено"));
                Assert.That(practiceInDb?.Finalgrade, Is.EqualTo("A"));
                Assert.That(practiceInDb?.Type, Is.EqualTo("TypeC"));
            });
    }

    /// <summary>
    /// Verifies that updating a non-existent practice returns BadRequest.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdatePractice_NonExistent_ReturnsBadRequest()
    {
        var practice = new Practice
        {
            Id = 999,
            Status = "NonExistent",
        };

        var result = await this.practicesQueries.UpdatePractice(practice);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.That(result, Is.InstanceOf<BadRequest>());
    }

    /// <summary>
    /// Verifies that deleting an existing practice works.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeletePractice_ExistingId_RemovesPractice()
    {
        var result = await this.practicesQueries.DeletePractice(1);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.That(result, Is.InstanceOf<Ok>());

        var deletedPractice = await this.dbContext.Practices.FindAsync(1);
        Assert.That(deletedPractice, Is.Null);
    }

    /// <summary>
    /// Verifies that deleting a non-existent practice throws an exception.
    /// </summary>
    [Test]
    public void DeletePractice_NonExistent_ThrowsException()
    {
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await this.practicesQueries.DeletePractice(999));
    }
}
