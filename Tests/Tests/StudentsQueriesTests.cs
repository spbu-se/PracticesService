// <copyright file="StudentsQueriesTests.cs" company="Gleb Kargin">
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
/// Unit tests for <see cref="StudentsQueries"/> using in-memory database.
/// </summary>
[TestFixture]
public class StudentsQueriesTests
{
    private CoreContext dbContext;
    private StudentsQueries studentsQueries;

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

        this.dbContext.Students.Add(
            new Student
        {
            Id = 1,
            Userid = "user-1",
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "A",
            Groupid = null,
        });

        this.dbContext.SaveChanges();
        this.studentsQueries = new StudentsQueries(this.dbContext);
    }

    /// <summary>
    /// Disposes the database context.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        this.dbContext.Dispose();
    }

    /// <summary>
    /// Verifies that all students are returned when no ID is provided.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetStudents_WithoutId_ReturnsAllStudents()
    {
        var result = await this.studentsQueries.GetStudents();

        Assert.Multiple(
            () =>
        {
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().FirstName, Is.EqualTo("John"));
        });
    }

    /// <summary>
    /// Verifies that a student is returned by ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetStudents_WithId_ReturnsMatchingStudent()
    {
        var result = await this.studentsQueries.GetStudents(1);

        Assert.Multiple(
            () =>
        {
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Verifies that a student is returned by UserId.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetStudentByUserId_ExistingUserId_ReturnsStudent()
    {
        var result = await this.studentsQueries.GetStudentByUserId("user-1");

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.FirstName, Is.EqualTo("John"));
    }

    /// <summary>
    /// Verifies that InsertOrUpdateStudent inserts a new student.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InsertOrUpdateStudent_NewStudent_InsertsSuccessfully()
    {
        var student = new Student
        {
            Id = 2,
            Userid = "user-2",
            FirstName = "Jane",
            LastName = "Smith",
        };

        var result = await this.studentsQueries.InsertOrUpdateStudent(student);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.Multiple(
            () =>
        {
            Assert.That(result.ToString(), Does.Contain("Ok"));
            Assert.That(this.dbContext.Students.Find(2), Is.Not.Null);
        });
    }

    /// <summary>
    /// Verifies that InsertOrUpdateStudent updates an existing student.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InsertOrUpdateStudent_ExistingStudent_UpdatesSuccessfully()
    {
        var updated = new Student
        {
            Id = 1,
            FirstName = "Johnny",
            LastName = "Doe",
        };

        var result = await this.studentsQueries.InsertOrUpdateStudent(updated);

        Assert.That(result, Is.InstanceOf<IResult>());
        Assert.Multiple(
            () =>
        {
            Assert.That(result.ToString(), Does.Contain("Ok"));
            Assert.That(this.dbContext.Students.Find(1)?.FirstName, Is.EqualTo("Johnny"));
        });
    }

    /// <summary>
    /// Verifies that UpdateStudent returns BadRequest for non-existent student.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateStudent_NonExistent_ReturnsBadRequest()
    {
        var nonExistent = new Student
        {
            Id = 999,
            FirstName = "Ghost",
        };

        var result = await this.studentsQueries.UpdateStudent(nonExistent);

        Assert.That(result.ToString(), Does.Contain("BadRequest"));
    }

    /// <summary>
    /// Verifies that an existing student can be deleted.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteStudent_ExistingId_RemovesStudent()
    {
        var result = await this.studentsQueries.DeleteStudent(1);

        Assert.Multiple(
            () =>
        {
            Assert.That(result.ToString(), Does.Contain("Ok"));
            Assert.That(this.dbContext.Students.Find(1), Is.Null);
        });
    }
}
