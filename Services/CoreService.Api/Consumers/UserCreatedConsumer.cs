// <copyright file="UserCreatedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Consumers
{
    using Contracts;
    using CoreService.Core;
    using CoreService.Core.Models;
    using CoreService.Core.Queries;
    using MassTransit;

    /// <summary>
    /// Consumer of user creation event.
    /// </summary>
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly LecturersQueries lecturersQueries;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCreatedConsumer"/> class.
        /// </summary>
        /// <param name="context">Core DB context.</param>
        public UserCreatedConsumer(CoreContext context)
        {
            this.lecturersQueries = new LecturersQueries(context);
        }

        /// <summary>
        /// Consumes Event.
        /// </summary>
        /// <param name="context">Consume event context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            if (context.Message.Roles.Contains("Научный руководитель"))
            {
                var lecturer = new Lecturer()
                {
                    Userid = context.Message.UserId,
                    FirstName = context.Message.FirstName,
                    LastName = context.Message.LastName,
                    MiddleName = context.Message.MiddleName,
                };
                await this.lecturersQueries.InsertOrUpdateLecturer(lecturer);
            }
        }
    }
}