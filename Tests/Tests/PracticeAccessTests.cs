// <copyright file="PracticeAccessTests.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Tests;

using System.Security.Claims;
using Contracts;
using NUnit.Framework;
using Shared.Access;

/// <summary>
/// Unit tests for the practice access rules.
/// </summary>
[TestFixture]
public class PracticeAccessTests
{
    private static readonly PracticeParticipants Practice = new("student-1", "supervisor-1", "consultant-1");

    /// <summary>
    /// Tests the rules for every kind of caller.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <param name="read">Expected: may read.</param>
    /// <param name="edit">Expected: may edit.</param>
    /// <param name="grade">Expected: may set the grade.</param>
    /// <param name="feedback">Expected: may leave a feedback.</param>
    /// <param name="listAll">Expected: may list all practices.</param>
    [TestCaseSource(nameof(Callers))]
    public void Rules_MatchTheMatrix(ClaimsPrincipal user, bool read, bool edit, bool grade, bool feedback, bool listAll)
    {
        Assert.Multiple(() =>
        {
            Assert.That(PracticeAccess.CanRead(user, Practice), Is.EqualTo(read), "read");
            Assert.That(PracticeAccess.CanContribute(user, Practice), Is.EqualTo(read), "contribute");
            Assert.That(PracticeAccess.CanEdit(user, Practice), Is.EqualTo(edit), "edit");
            Assert.That(PracticeAccess.CanDelete(user, Practice), Is.EqualTo(edit), "delete");
            Assert.That(PracticeAccess.CanGrade(user, Practice), Is.EqualTo(grade), "grade");
            Assert.That(PracticeAccess.CanLeaveFeedback(user, Practice), Is.EqualTo(feedback), "feedback");
            Assert.That(PracticeAccess.CanListAll(user), Is.EqualTo(listAll), "list");
        });
    }

    /// <summary>
    /// A practice without a consultant must not be opened to a caller without an id.
    /// </summary>
    [Test]
    public void Participants_WithoutConsultant_DoNotMatchAnonymous()
    {
        var practice = new PracticeParticipants("student-1", "supervisor-1", null);
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.That(PracticeAccess.RelationOf(anonymous, practice), Is.EqualTo(PracticeRelation.None));
    }

    /// <summary>
    /// The relation is found from the user id in the token.
    /// </summary>
    [Test]
    public void RelationOf_UsesTheUserIdFromTheToken()
    {
        Assert.Multiple(() =>
        {
            Assert.That(PracticeAccess.RelationOf(User("student-1", UserRoleType.Student), Practice), Is.EqualTo(PracticeRelation.Student));
            Assert.That(PracticeAccess.RelationOf(User("supervisor-1", UserRoleType.Supervisor), Practice), Is.EqualTo(PracticeRelation.Supervisor));
            Assert.That(PracticeAccess.RelationOf(User("consultant-1", UserRoleType.Consultant), Practice), Is.EqualTo(PracticeRelation.Consultant));
            Assert.That(PracticeAccess.RelationOf(User("someone", UserRoleType.Student), Practice), Is.EqualTo(PracticeRelation.None));
        });
    }

    /// <summary>
    /// The name is shown as the user interface shows it.
    /// </summary>
    [Test]
    public void DisplayName_JoinsLastFirstAndMiddleName()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Surname, "Иванов"),
                new Claim(ClaimTypes.GivenName, "Иван"),
                new Claim("middle_name", string.Empty),
                new Claim(ClaimTypes.Name, "ivanov@example.com"),
            },
            "test"));

        Assert.That(user.DisplayName(), Is.EqualTo("Иванов Иван"));
    }

    /// <summary>
    /// Without name claims the login is used.
    /// </summary>
    [Test]
    public void DisplayName_WithoutNameClaims_FallsBackToLogin()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "ivanov@example.com") }, "test"));

        Assert.That(user.DisplayName(), Is.EqualTo("ivanov@example.com"));
    }

    /// <summary>
    /// Gets the callers with what each of them may do with the practice.
    /// </summary>
    /// <returns>Test cases: name, caller, read, edit, grade, leave a feedback, list all.</returns>
    private static IEnumerable<TestCaseData> Callers()
    {
        yield return Case("practice student", User("student-1", UserRoleType.Student), true, true, false, false, false);
        yield return Case("other student", User("student-2", UserRoleType.Student), false, false, false, false, false);
        yield return Case("practice supervisor", User("supervisor-1", UserRoleType.Supervisor), true, true, true, true, false);
        yield return Case("other supervisor", User("supervisor-2", UserRoleType.Supervisor), false, false, false, false, false);
        yield return Case("practice consultant", User("consultant-1", UserRoleType.Consultant), true, false, false, true, false);
        yield return Case("other consultant", User("consultant-2", UserRoleType.Consultant), false, false, false, false, false);
        yield return Case("practice leader", User("leader-1", UserRoleType.PracticeLeader), true, true, true, true, true);
        yield return Case("administrator", User("admin-1", UserRoleType.Administrator), true, true, true, true, true);
        yield return Case("reviewer", User("reviewer-1", UserRoleType.Reviewer), true, false, false, false, true);
        yield return Case("another service", new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "internal-service") }, "InternalKey")), true, false, false, false, false);
        yield return Case("anonymous", new ClaimsPrincipal(new ClaimsIdentity()), false, false, false, false, false);
    }

    private static TestCaseData Case(string name, ClaimsPrincipal user, bool read, bool edit, bool grade, bool feedback, bool listAll)
        => new TestCaseData(user, read, edit, grade, feedback, listAll).SetName($"Rules_{name.Replace(' ', '_')}");

    private static ClaimsPrincipal User(string id, UserRoleType role)
        => new(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Role, RoleNames.GetName(role)),
            },
            "test"));
}
