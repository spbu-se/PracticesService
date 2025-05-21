// <copyright file="UserResolverServiceTests.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using CoreService.Api.Services;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;

    /// <summary>
    /// Test class for <see cref="UserResolverService"/>.
    /// </summary>
    [TestFixture]
    public class UserResolverServiceTests
    {
        private Mock<IHttpClientFactory> httpClientFactoryMock;
        private Mock<HttpMessageHandler> httpMessageHandlerMock;
        private UserResolverService userResolverService;
        private HttpClient httpClient;

        /// <summary>
        /// Initializes test setup before each test method.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            this.httpClient = new HttpClient(this.httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://authservice.local/"),
            };

            this.httpClientFactoryMock = new Mock<IHttpClientFactory>();
            this.httpClientFactoryMock
                .Setup(f => f.CreateClient("AuthService"))
                .Returns(this.httpClient);

            this.userResolverService = new UserResolverService(this.httpClientFactoryMock.Object);
        }

        /// <summary>
        /// Cleans up test resources after each test method.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.httpClient.Dispose();
        }

        /// <summary>
        /// Tests that GetUserAsync returns UserDTO when response is successful.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetUserAsync_ReturnsUserDTO_WhenResponseIsSuccess()
        {
            var userId = Guid.NewGuid();
            var expectedUser = new UserDTO
            {
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
            };

            this.httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains($"user?userId={userId}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(
                    new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(expectedUser),
                });

            var actualUser = await this.userResolverService.GetUserAsync(userId);

            Assert.That(actualUser, Is.Not.Null);
            Assert.Multiple(
                () =>
            {
                Assert.That(actualUser!.UserName, Is.EqualTo(expectedUser.UserName));
                Assert.That(actualUser.FirstName, Is.EqualTo(expectedUser.FirstName));
                Assert.That(actualUser.LastName, Is.EqualTo(expectedUser.LastName));
            });
        }

        /// <summary>
        /// Tests that GetUserAsync returns null when response is not successful.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetUserAsync_ReturnsNull_WhenResponseIsNotSuccess()
        {
            var userId = Guid.NewGuid();

            this.httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                });

            var user = await this.userResolverService.GetUserAsync(userId);

            Assert.That(user, Is.Null);
        }

        /// <summary>
        /// Tests that GetUserIdAsync returns userId string when response is successful.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetUserIdAsync_ReturnsUserIdString_WhenResponseIsSuccess()
        {
            const string username = "testuser";
            var expectedUserId = Guid.NewGuid().ToString();

            this.httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains($"userId?userName={username}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(
                    new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedUserId),
                });

            var actualUserId = await this.userResolverService.GetUserIdAsync(username);

            Assert.That(actualUserId, Is.EqualTo(expectedUserId));
        }

        /// <summary>
        /// Tests that GetUserIdAsync returns null when response is not successful.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetUserIdAsync_ReturnsNull_WhenResponseIsNotSuccess()
        {
            const string username = "nonexistent";

            this.httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(
                    new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                });

            var userId = await this.userResolverService.GetUserIdAsync(username);

            Assert.That(userId, Is.Null);
        }
    }
}