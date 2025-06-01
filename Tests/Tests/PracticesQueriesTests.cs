// <copyright file="PracticesQueriesTests.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CoreService.Api.Core;
    using CoreService.Api.Core.Models;
    using CoreService.Api.Core.Queries;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;

    /// <summary>
    /// Unit tests for <see cref="PracticesQueries"/> using in-memory database.
    /// </summary>
    [TestFixture]
    public class PracticesQueriesTests
    {
        private CoreContext dbContext;
        private PracticesQueries practicesQueries;

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

            var student = new Student
            {
                Id = 1,
                Userid = "user-1",
                FirstName = "John",
                LastName = "Doe",
            };

            this.dbContext.Students.Add(student);

            var supervisor = new Lecturer()
            {
                Id = 1,
                Userid = "user-1",
                FirstName = "John",
                LastName = "Doe",
            };

            this.dbContext.Lecturers.Add(supervisor);

            var consultant = new Consultant()
            {
                Id = 1,
                Userid = "user-1",
                FirstName = "John",
                LastName = "Doe",
                Contact = "Contact",
            };

            this.dbContext.Consultants.Add(consultant);

            var theme = new Theme
            {
                Id = 1,
                Title = "Theme B",
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
                Finalgrade = "B",
                Updateddate = DateTime.UtcNow,
                Type = "TypeA",
                Consultantid = 1,
                Supervisorid = 1,
                Themeid = 1,
            };

            this.dbContext.Practices.Add(practice);
            this.dbContext.SaveChanges();

            this.practicesQueries = new PracticesQueries(this.dbContext);
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
        /// Verifies that all practices are returned when no ID is specified.
        /// </summary>
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
        public async Task GetQueriedPractices_WithValidUserId_ReturnsPractices()
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
        /// <param name="userId">User Id.</param>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [TestCase("")]
        public async Task GetQueriedPractices_WithEmptyUserId_ReturnsEmptyList(string userId)
        {
            var result = await this.practicesQueries.GetPracticesByStudent(userId);

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
                Status = "Completed",
                Finalgrade = "A",
                Type = "TypeC",
                Studentid = 1,
                Consultantid = 5,
                Supervisorid = 6,
                Themeid = 3,
            };

            var result = await this.practicesQueries.UpdatePractice(updatedPractice);

            Assert.That(result.ToString(), Does.Contain("Ok"));
            var practiceInDb = this.dbContext.Practices.Find(1);
            Assert.Multiple(
                () =>
            {
                Assert.That(practiceInDb?.Status, Is.EqualTo("Completed"));
                Assert.That(practiceInDb?.Finalgrade, Is.EqualTo("A"));
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

            Assert.That(result.ToString(), Does.Contain("BadRequest"));
        }

        /// <summary>
        /// Verifies that deleting an existing practice works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task DeletePractice_ExistingId_RemovesPractice()
        {
            var result = await this.practicesQueries.DeletePractice(1);

            Assert.Multiple(
                () =>
            {
                Assert.That(result.ToString(), Does.Contain("Ok"));
                Assert.That(this.dbContext.Practices.Find(1), Is.Null);
            });
        }
    }
}
