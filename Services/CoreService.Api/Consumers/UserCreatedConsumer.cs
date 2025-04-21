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
        private readonly StudentsQueries studentsQueries;
        private readonly ConsultantsQueries consultantsQueries;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCreatedConsumer"/> class.
        /// </summary>
        /// <param name="context">Core DB context.</param>
        public UserCreatedConsumer(CoreContext context)
        {
            this.lecturersQueries = new LecturersQueries(context);
            this.studentsQueries = new StudentsQueries(context);
            this.consultantsQueries = new ConsultantsQueries(context);
        }

        /// <summary>
        /// Consumes Event.
        /// </summary>
        /// <param name="context">Consume event context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            if (context.Message.Roles.Contains(RoleNames.GetName(UserRoleType.Supervisor)))
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
            else if (context.Message.Roles.Contains(RoleNames.GetName(UserRoleType.Student)))
            {
                var student = new Student()
                {
                    Userid = context.Message.UserId,
                    FirstName = context.Message.FirstName,
                    LastName = context.Message.LastName,
                    MiddleName = context.Message.MiddleName,
                };
                await this.studentsQueries.InsertOrUpdateStudent(student);
            }
            else if (context.Message.Roles.Contains(RoleNames.GetName(UserRoleType.Consultant)))
            {
                var consultant = new Consultant()
                {
                    Userid = context.Message.UserId,
                    FirstName = context.Message.FirstName,
                    LastName = context.Message.LastName,
                    MiddleName = context.Message.MiddleName,
                };
                await this.consultantsQueries.InsertOrUpdateConsultant(consultant);
            }
        }
    }
}