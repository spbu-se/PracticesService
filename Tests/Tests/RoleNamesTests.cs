// <copyright file="RoleNamesTests.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Tests;

using Contracts;
using NUnit.Framework;

/// <summary>
/// Unit tests for the <see cref="RoleNames"/> class.
/// </summary>
[TestFixture]
public class RoleNamesTests
{
    /// <summary>
    /// Tests that all expected role names are returned by <see cref="RoleNames.GetAllRoleNames"/>.
    /// </summary>
    [Test]
    public void GetAllRoleNames_ReturnsAllDefinedRoleNames()
    {
        var expectedNames = new[]
        {
            "Студент",
            "Научный руководитель",
            "Консультант",
            "Руководитель практики",
            "Рецензент",
            "Администратор",
        };

        var actualNames = RoleNames.GetAllRoleNames();

        Assert.That(actualNames, Is.EquivalentTo(expectedNames));
    }

    /// <summary>
    /// Tests that <see cref="RoleNames.GetName"/> returns the correct name for each defined role.
    /// <param name="role">Role type.</param>
    /// <param name="expectedName">Expected role name.</param>
    /// </summary>
    [TestCase(UserRoleType.Student, "Студент")]
    [TestCase(UserRoleType.Supervisor, "Научный руководитель")]
    [TestCase(UserRoleType.Consultant, "Консультант")]
    [TestCase(UserRoleType.PracticeLeader, "Руководитель практики")]
    [TestCase(UserRoleType.Reviewer, "Рецензент")]
    [TestCase(UserRoleType.Administrator, "Администратор")]
    public void GetName_ReturnsCorrectName_ForKnownRole(UserRoleType role, string expectedName)
    {
        var actualName = RoleNames.GetName(role);

        Assert.That(actualName, Is.EqualTo(expectedName));
    }

    /// <summary>
    /// Tests that <see cref="RoleNames.GetName"/> returns enum.ToString() for unknown role values.
    /// </summary>
    [Test]
    public void GetName_ReturnsEnumToString_ForUnknownRole()
    {
        const UserRoleType unknownRole = (UserRoleType)999;
        var result = RoleNames.GetName(unknownRole);

        Assert.That(result, Is.EqualTo(unknownRole.ToString()));
    }

    /// <summary>
    /// Tests that <see cref="RoleNames.IsCorrectRole"/> returns true for valid role names.
    /// <param name="roleName">Role name.</param>
    /// </summary>
    [TestCase("Студент")]
    [TestCase("Научный руководитель")]
    [TestCase("Консультант")]
    [TestCase("Руководитель практики")]
    [TestCase("Рецензент")]
    [TestCase("Администратор")]
    public void IsCorrectRole_ReturnsTrue_ForValidRoleNames(string roleName)
    {
        var result = RoleNames.IsCorrectRole(roleName);

        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests that <see cref="RoleNames.IsCorrectRole"/> returns false for invalid role names.
    /// <param name="roleName">Role name.</param>
    /// </summary>
    [TestCase("InvalidRole")]
    [TestCase("")]
    [TestCase("студент")]
    [TestCase("admin")]
    public void IsCorrectRole_ReturnsFalse_ForInvalidRoleNames(string roleName)
    {
        var result = RoleNames.IsCorrectRole(roleName);

        Assert.That(result, Is.False);
    }
}
