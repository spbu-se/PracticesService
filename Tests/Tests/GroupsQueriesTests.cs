// <copyright file="GroupsQueriesTests.cs" company="Gleb Kargin">
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
    /// Unit tests for <see cref="GroupsQueries"/> using in-memory database.
    /// </summary>
    [TestFixture]
    public class GroupsQueriesTests
    {
        private CoreContext dbContext = null!;
        private GroupsQueries groupsQueries = null!;

        /// <summary>
        /// Sets up in-memory database and adds sample data.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CoreContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            this.dbContext = new CoreContext(options);
            this.dbContext.Database.EnsureCreated();

            this.dbContext.Groups.Add(
                new Group
            {
                Id = 1,
                Name = "Group A",
                Program = "Program 1",
                Year = 2023,
            });

            this.dbContext.SaveChanges();

            this.groupsQueries = new GroupsQueries(this.dbContext);
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
        /// Tests getting all groups without specifying an ID.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetGroups_WithoutId_ReturnsAllGroups()
        {
            var result = await this.groupsQueries.GetGroups();

            Assert.Multiple(
                () =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Group A"));
            });
        }

        /// <summary>
        /// Tests getting groups by specific ID.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetGroups_WithId_ReturnsMatchingGroup()
        {
            var result = await this.groupsQueries.GetGroups(1);

            Assert.Multiple(
                () =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Id, Is.EqualTo(1));
            });
        }

        /// <summary>
        /// Tests inserting a new group.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task InsertGroup_NewGroup_AddsGroup()
        {
            var newGroup = new Group
            {
                Id = 2,
                Name = "Group B",
                Program = "Program 2",
                Year = 2024,
            };

            var id = await this.groupsQueries.InsertGroup(newGroup);

            Assert.That(id, Is.EqualTo(2));
            var groupInDb = this.dbContext.Groups.Find(2);
            Assert.That(groupInDb, Is.Not.Null);
            Assert.That(groupInDb?.Name, Is.EqualTo("Group B"));
        }

        /// <summary>
        /// Tests updating an existing group.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task UpdateGroup_ExistingGroup_UpdatesGroup()
        {
            var updatedGroup = new Group
            {
                Id = 1,
                Name = "Updated Group",
                Program = "Updated Program",
                Year = 2025,
            };

            var result = await this.groupsQueries.UpdateGroup(updatedGroup);

            Assert.That(result.ToString(), Does.Contain("Ok"));
            var groupInDb = this.dbContext.Groups.Find(1);
            Assert.Multiple(
                () =>
            {
                Assert.That(groupInDb?.Name, Is.EqualTo("Updated Group"));
                Assert.That(groupInDb?.Program, Is.EqualTo("Updated Program"));
                Assert.That(groupInDb?.Year, Is.EqualTo(2025));
            });
        }

        /// <summary>
        /// Tests updating a group that does not exist returns BadRequest.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task UpdateGroup_NonExistentGroup_ReturnsBadRequest()
        {
            var group = new Group
            {
                Id = 999,
                Name = "Nonexistent Group",
                Program = "None",
                Year = 2022,
            };

            var result = await this.groupsQueries.UpdateGroup(group);

            Assert.That(result.ToString(), Does.Contain("BadRequest"));
        }

        /// <summary>
        /// Tests deleting an existing group.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task DeleteGroup_ExistingId_RemovesGroup()
        {
            var result = await this.groupsQueries.DeleteGroup(1);

            Assert.Multiple(
                () =>
            {
                Assert.That(result.ToString(), Does.Contain("Ok"));
                Assert.That(this.dbContext.Groups.Find(1), Is.Null);
            });
        }

        /// <summary>
        /// Tests deleting a non-existent group throws exception.
        /// </summary>
        [Test]
        public void DeleteGroup_NonExistentId_ThrowsException()
        {
            Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                await this.groupsQueries.DeleteGroup(999));
        }
    }
}
