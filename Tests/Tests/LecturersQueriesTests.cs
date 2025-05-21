// <copyright file="LecturersQueriesTests.cs" company="Gleb Kargin">
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
    /// Unit tests for <see cref="LecturersQueries"/> using in-memory database.
    /// </summary>
    [TestFixture]
    public class LecturersQueriesTests
    {
        private CoreContext dbContext;
        private LecturersQueries lecturersQueries;

        /// <summary>
        /// Sets up the in-memory database and initializes test data.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CoreContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            this.dbContext = new CoreContext(options);
            this.dbContext.Database.EnsureCreated();

            this.dbContext.Lecturers.Add(
                new Lecturer
            {
                Id = 1,
                Userid = "lecturer-1",
                FirstName = "Anna",
                LastName = "Smith",
                MiddleName = "M.",
                Department = "Mathematics",
                Cansupervisevkr = true,
            });

            this.dbContext.SaveChanges();

            this.lecturersQueries = new LecturersQueries(this.dbContext);
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
        /// Tests getting all lecturers without specifying an ID.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetLecturers_WithoutId_ReturnsAllLecturers()
        {
            var result = await this.lecturersQueries.GetLecturers();

            Assert.Multiple(
                () =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().FirstName, Is.EqualTo("Anna"));
            });
        }

        /// <summary>
        /// Tests getting lecturer by specific ID.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetLecturers_WithId_ReturnsMatchingLecturer()
        {
            var result = await this.lecturersQueries.GetLecturers(1);

            Assert.Multiple(
                () =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Id, Is.EqualTo(1));
            });
        }

        /// <summary>
        /// Tests getting lecturer by UserId.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetLecturerByUserId_ValidUserId_ReturnsLecturer()
        {
            var result = await this.lecturersQueries.GetLecturerByUserId("lecturer-1");

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.FirstName, Is.EqualTo("Anna"));
        }

        /// <summary>
        /// Tests getting lecturer by UserId when no match exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetLecturerByUserId_InvalidUserId_ReturnsNull()
        {
            var result = await this.lecturersQueries.GetLecturerByUserId("invalid-user");

            Assert.That(result, Is.Null);
        }

        /// <summary>
        /// Tests inserting a new lecturer.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task InsertOrUpdateLecturer_NewLecturer_AddsLecturer()
        {
            var newLecturer = new Lecturer
            {
                Id = 2,
                Userid = "lecturer-2",
                FirstName = "Bob",
                LastName = "Brown",
                Department = "Physics",
                Cansupervisevkr = false,
            };

            var result = await this.lecturersQueries.InsertOrUpdateLecturer(newLecturer);

            Assert.That(result.ToString(), Does.Contain("Ok"));
            var lecturerInDb = this.dbContext.Lecturers.Find(2);
            Assert.That(lecturerInDb, Is.Not.Null);
            Assert.That(lecturerInDb?.FirstName, Is.EqualTo("Bob"));
        }

        /// <summary>
        /// Tests updating an existing lecturer.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task InsertOrUpdateLecturer_ExistingLecturer_UpdatesLecturer()
        {
            var updatedLecturer = new Lecturer
            {
                Id = 1,
                FirstName = "Anna Updated",
                LastName = "Smith",
                Department = "Computer Science",
                Cansupervisevkr = false,
            };

            var result = await this.lecturersQueries.InsertOrUpdateLecturer(updatedLecturer);

            Assert.That(result.ToString(), Does.Contain("Ok"));
            var lecturerInDb = this.dbContext.Lecturers.Find(1);
            Assert.Multiple(
                () =>
            {
                Assert.That(lecturerInDb?.FirstName, Is.EqualTo("Anna Updated"));
                Assert.That(lecturerInDb?.Department, Is.EqualTo("Computer Science"));
                Assert.That(lecturerInDb?.Cansupervisevkr, Is.False);
            });
        }

        /// <summary>
        /// Tests updating a lecturer that does not exist returns BadRequest.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task UpdateLecturer_NonExistentLecturer_ReturnsBadRequest()
        {
            var lecturer = new Lecturer
            {
                Id = 999,
                FirstName = "Ghost",
            };

            var result = await this.lecturersQueries.UpdateLecturer(lecturer);

            Assert.That(result.ToString(), Does.Contain("BadRequest"));
        }

        /// <summary>
        /// Tests updating a lecturer successfully.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task UpdateLecturer_ValidLecturer_ReturnsOk()
        {
            var updatedLecturer = new Lecturer
            {
                Id = 1,
                FirstName = "Anna Updated",
                LastName = "Smith Updated",
                Department = "Chemistry",
                Cansupervisevkr = true,
            };

            var result = await this.lecturersQueries.UpdateLecturer(updatedLecturer);

            Assert.That(result.ToString(), Does.Contain("Ok"));
            var lecturerInDb = this.dbContext.Lecturers.Find(1);
            Assert.Multiple(
                () =>
            {
                Assert.That(lecturerInDb?.FirstName, Is.EqualTo("Anna Updated"));
                Assert.That(lecturerInDb?.LastName, Is.EqualTo("Smith Updated"));
                Assert.That(lecturerInDb?.Department, Is.EqualTo("Chemistry"));
                Assert.That(lecturerInDb?.Cansupervisevkr, Is.True);
            });
        }

        /// <summary>
        /// Tests deleting an existing lecturer.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task DeleteLecturer_ExistingId_RemovesLecturer()
        {
            var result = await this.lecturersQueries.DeleteLecturer(1);

            Assert.Multiple(
                () =>
            {
                Assert.That(result.ToString(), Does.Contain("Ok"));
                Assert.That(this.dbContext.Lecturers.Find(1), Is.Null);
            });
        }

        /// <summary>
        /// Tests deleting a non-existing lecturer does not throw and returns Ok.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task DeleteLecturer_NonExistentId_ReturnsOk()
        {
            var result = await this.lecturersQueries.DeleteLecturer(999);

            Assert.That(result.ToString(), Does.Contain("Ok"));
        }
    }
}
