using Domain.Entities;
using Domain.Enums;

namespace FamilyTree.Tests.Helpers;

/// <summary>
/// Tiny factories for the entities/DTOs the tests build over and over.
/// Each method takes optional overrides so individual tests can tweak only
/// the fields they care about — keeps test bodies focused on behaviour.
/// </summary>
internal static class TestData
{
    public static Member Member(
        Guid? id = null,
        Guid? familyId = null,
        string firstName = "M",
        string lastName = "L",
        Gender gender = Gender.MALE,
        Guid? fatherId = null,
        Guid? motherId = null,
        Guid? spouseId = null,
        DateOnly? birthDay = null,
        DateOnly? deathDay = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            FamilyId = familyId ?? Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Gender = gender,
            FatherId = fatherId,
            MotherId = motherId,
            SpouseId = spouseId,
            BirthDay = birthDay ?? new DateOnly(1990, 1, 1),
            DeathDay = deathDay
        };

    public static Family Family(
        Guid? id = null,
        Guid? ownerId = null,
        string name = "Karimovlar",
        string familyName = "Karimov") =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            OwnerId = ownerId ?? Guid.NewGuid(),
            Name = name,
            FamilyName = familyName
        };

    public static User User(
        Guid? id = null,
        Guid? roleId = null,
        string firstName = "Test",
        string lastName = "User",
        string email = "test@example.com",
        string passwordHash = "hashed") =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            RoleId = roleId ?? Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = passwordHash
        };

    public static UserRole UserRole(
        Guid? id = null,
        string name = "Admin",
        string designedName = "ADMIN") =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            DesignedName = designedName
        };

    public static UserRolePermission UserRolePermission(
        Guid? id = null,
        Guid? userRoleId = null,
        Permission permission = Permission.GET_FAMILY) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            UserRoleId = userRoleId ?? Guid.NewGuid(),
            Permission = permission
        };
}
