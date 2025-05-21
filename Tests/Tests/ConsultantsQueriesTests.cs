// <copyright file="ConsultantsQueriesTests.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Tests;

using System;
using System.Linq;
using System.Threading.Tasks;
using CoreService.Api.Core;
using CoreService.Api.Core.Models;
using CoreService.Api.Core.Queries;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

/// <summary>
/// Unit tests for <see cref="ConsultantsQueries"/> using in-memory database.
/// </summary>
[TestFixture]
public class ConsultantsQueriesTests
{
    private CoreContext dbContext;
    private ConsultantsQueries consultantsQueries;

    /// <summary>
    /// Initializes test setup before each test method.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        this.dbContext = new CoreContext(options);
        this.consultantsQueries = new ConsultantsQueries(this.dbContext);

        this.dbContext.Consultants.Add(
            new Consultant
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "A",
            Contact = "john.doe@example.com",
            Userid = "user1",
        });
        this.dbContext.SaveChanges();
    }

    /// <summary>
    /// Cleans up test resources after each test method.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        this.dbContext.Dispose();
    }

    /// <summary>
    /// Tests that GetConsultants without ID returns all consultants.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetConsultants_WithoutId_ReturnsAll()
    {
        var result = await this.consultantsQueries.GetConsultants();

        Assert.Multiple(
            () =>
        {
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Userid, Is.EqualTo("user1"));
        });
    }

    /// <summary>
    /// Tests that GetConsultants with ID returns matching consultant.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetConsultants_WithId_ReturnsMatching()
    {
        var result = await this.consultantsQueries.GetConsultants(1);

        Assert.Multiple(
            () =>
        {
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Tests that GetConsultantByUserId with existing user ID returns consultant.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetConsultantByUserId_ExistingUserId_ReturnsConsultant()
    {
        var result = await this.consultantsQueries.GetConsultantByUserId("user1");

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.FirstName, Is.EqualTo("John"));
    }

    /// <summary>
    /// Tests that GetConsultantByUserId with non-existing user ID returns null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetConsultantByUserId_NonExistingUserId_ReturnsNull()
    {
        var result = await this.consultantsQueries.GetConsultantByUserId("nonexistent");

        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that InsertOrUpdateConsultant with new consultant adds the consultant.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InsertOrUpdateConsultant_NewConsultant_AddsConsultant()
    {
        var newConsultant = new Consultant
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            MiddleName = "B",
            Contact = "jane.smith@example.com",
            Userid = "user2",
        };

        var result = await this.consultantsQueries.InsertOrUpdateConsultant(newConsultant);
        Assert.Multiple(
            () =>
        {
            Assert.That(result.ToString(), Does.Contain("Ok"));
            Assert.That(this.dbContext.Consultants.Any(c => c.Id == 2), Is.True);
        });
    }

    /// <summary>
    /// Tests that InsertOrUpdateConsultant with existing consultant updates the consultant.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InsertOrUpdateConsultant_ExistingConsultant_UpdatesConsultant()
    {
        var updatedConsultant = new Consultant
        {
            Id = 1,
            FirstName = "John Updated",
            LastName = "Doe Updated",
            MiddleName = "A Updated",
            Contact = "updated@example.com",
            Userid = "user1",
        };

        var result = await this.consultantsQueries.InsertOrUpdateConsultant(updatedConsultant);

        Assert.That(result.ToString(), Does.Contain("Ok"));

        var inDb = await this.dbContext.Consultants.FindAsync(1);
        Assert.Multiple(
            () =>
        {
            Assert.That(inDb?.FirstName, Is.EqualTo("John Updated"));
            Assert.That(inDb?.Contact, Is.EqualTo("updated@example.com"));
        });
    }

    /// <summary>
    /// Tests that UpdateConsultant with existing consultant updates fields.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateConsultant_ExistingConsultant_UpdatesFields()
    {
        var consultant = new Consultant
        {
            Id = 1,
            FirstName = "NewFirst",
            LastName = "NewLast",
            MiddleName = "NewMiddle",
            Contact = "newcontact@example.com",
        };

        var result = await this.consultantsQueries.UpdateConsultant(consultant);

        Assert.That(result.ToString(), Does.Contain("Ok"));

        var inDb = await this.dbContext.Consultants.FindAsync(1);
        Assert.Multiple(
            () =>
        {
            Assert.That(inDb?.FirstName, Is.EqualTo("NewFirst"));
            Assert.That(inDb?.Contact, Is.EqualTo("newcontact@example.com"));
        });
    }

    /// <summary>
    /// Tests that UpdateConsultant with non-existing consultant returns BadRequest.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateConsultant_NonExistingConsultant_ReturnsBadRequest()
    {
        var consultant = new Consultant
        {
            Id = 999,
            FirstName = "NonExist",
        };

        var result = await this.consultantsQueries.UpdateConsultant(consultant);

        Assert.That(result.ToString(), Does.Contain("BadRequest"));
    }

    /// <summary>
    /// Tests that DeleteConsultant with existing ID deletes the consultant.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteConsultant_ExistingId_DeletesConsultant()
    {
        var result = await this.consultantsQueries.DeleteConsultant(1);

        Assert.Multiple(
            () =>
        {
            Assert.That(result.ToString(), Does.Contain("Ok"));
            Assert.That(this.dbContext.Consultants.Find(1), Is.Null);
        });
    }

    /// <summary>
    /// Tests that DeleteConsultant with non-existing ID throws exception.
    /// </summary>
    [Test]
    public void DeleteConsultant_NonExistingId_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            await this.consultantsQueries.DeleteConsultant(999));
    }
}