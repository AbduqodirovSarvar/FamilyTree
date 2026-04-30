using Application.Common.Interfaces.Repositories;
using Application.Common.Models.ViewModels.Tree;
using Application.Features.Family.Queries.GetFamilyTree;
using Domain.Enums;
using FamilyEntity = Domain.Entities.Family;
using FamilyTree.Tests.Helpers;
using MemberEntity = Domain.Entities.Member;

namespace FamilyTree.Tests.Features.Family;

/// <summary>
/// BuildForest is the algorithm-heavy part of the codebase: it picks roots,
/// joins spouses (forward, reverse, implicit-via-shared-child), and renders
/// polygamy as separate hubs. These tests cover each branch of that logic.
/// </summary>
public class GetFamilyTreeQueryHandlerTests
{
    private readonly Mock<IMemberRepository> _members = new();
    private readonly Mock<IFamilyRepository> _families = new();
    private readonly Guid _familyId = Guid.NewGuid();

    private GetFamilyTreeQueryHandler CreateSut() => new(_members.Object, _families.Object);

    private void Setup(IEnumerable<MemberEntity> members)
    {
        _families.Setup(r => r.GetByIdAsync(_familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Family(id: _familyId));
        _members.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<MemberEntity, bool>>>(),
                                          It.IsAny<CancellationToken>()))
            .ReturnsAsync(members.ToList());
    }

    [Fact]
    public async Task Handle_FamilyIdEmpty_ReturnsFail()
    {
        var sut = CreateSut();

        var response = await sut.Handle(new GetFamilyTreeQuery { FamilyId = Guid.Empty }, default);

        response.Success.Should().BeFalse();
        response.Message.Should().Contain("FamilyId");
    }

    [Fact]
    public async Task Handle_FamilyMissing_Throws()
    {
        _families.Setup(r => r.GetByIdAsync(_familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyEntity?)null);
        var sut = CreateSut();

        var act = () => sut.Handle(new GetFamilyTreeQuery { FamilyId = _familyId }, default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_NoMembers_ReturnsEmptyForest()
    {
        Setup([]);
        var sut = CreateSut();

        var response = await sut.Handle(new GetFamilyTreeQuery { FamilyId = _familyId }, default);

        response.Success.Should().BeTrue();
        response.Data!.Roots.Should().BeEmpty();
        response.Data.TotalMembers.Should().Be(0);
    }

    [Fact]
    public async Task Handle_SingleOrphan_BecomesRoot()
    {
        var only = TestData.Member(familyId: _familyId);
        Setup([only]);
        var sut = CreateSut();

        var response = await sut.Handle(new GetFamilyTreeQuery { FamilyId = _familyId }, default);

        response.Data!.Roots.Should().HaveCount(1);
        response.Data.Roots[0].Primary.Id.Should().Be(only.Id);
    }

    [Fact]
    public async Task Handle_FatherWithChild_ChildSitsUnderFather()
    {
        var father = TestData.Member(familyId: _familyId, firstName: "Dad");
        var child = TestData.Member(familyId: _familyId, firstName: "Kid", fatherId: father.Id);
        Setup([father, child]);
        var sut = CreateSut();

        var response = await sut.Handle(new GetFamilyTreeQuery { FamilyId = _familyId }, default);

        response.Data!.Roots.Should().HaveCount(1);
        var root = response.Data.Roots[0];
        root.Primary.Id.Should().Be(father.Id);
        root.CommonChildren.Should().HaveCount(1);
        root.CommonChildren[0].Primary.Id.Should().Be(child.Id);
    }

    [Fact]
    public async Task Handle_ForwardSpouse_RendersSingleHubWithBoth()
    {
        var wife = TestData.Member(familyId: _familyId, gender: Gender.FEMALE);
        // Forward link: husband.SpouseId points at wife.
        var husband = TestData.Member(familyId: _familyId, gender: Gender.MALE, spouseId: wife.Id);
        Setup([husband, wife]);
        var sut = CreateSut();

        var response = await sut.Handle(new GetFamilyTreeQuery { FamilyId = _familyId }, default);

        // Both members must appear under one node (one as primary, one as a spouse-group).
        response.Data!.Roots.Should().HaveCount(1);
        var root = response.Data.Roots[0];
        root.Spouses.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ReverseSpouse_StillJoinsAsHub()
    {
        // Only the wife points at the husband — used to render two disconnected hubs.
        var husband = TestData.Member(familyId: _familyId, gender: Gender.MALE);
        var wife = TestData.Member(familyId: _familyId, gender: Gender.FEMALE, spouseId: husband.Id);
        Setup([husband, wife]);
        var sut = CreateSut();

        var response = await sut.Handle(new GetFamilyTreeQuery { FamilyId = _familyId }, default);

        response.Data!.Roots.Should().HaveCount(1);
        response.Data.Roots[0].Spouses.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_SharedChildOnly_ImplicitSpouseLink()
    {
        // No explicit SpouseId on either parent — they're inferred via the child's
        // FatherId/MotherId both pointing at members of this family.
        var father = TestData.Member(familyId: _familyId, gender: Gender.MALE);
        var mother = TestData.Member(familyId: _familyId, gender: Gender.FEMALE);
        var child = TestData.Member(familyId: _familyId, fatherId: father.Id, motherId: mother.Id);
        Setup([father, mother, child]);
        var sut = CreateSut();

        var response = await sut.Handle(new GetFamilyTreeQuery { FamilyId = _familyId }, default);

        response.Data!.Roots.Should().HaveCount(1);
        var root = response.Data.Roots[0];
        // The "other parent" gets discovered through the shared child and joins as a spouse-group.
        root.Spouses.Should().HaveCount(1);
        // The child is grouped under that spouse, not as a CommonChild.
        root.Spouses[0].Children.Should().HaveCount(1);
        root.CommonChildren.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Polygamy_PrimaryHasMultipleSpouseGroups()
    {
        var husband = TestData.Member(familyId: _familyId, gender: Gender.MALE);
        var wife1 = TestData.Member(familyId: _familyId, gender: Gender.FEMALE);
        var wife2 = TestData.Member(familyId: _familyId, gender: Gender.FEMALE);
        var k1 = TestData.Member(familyId: _familyId, fatherId: husband.Id, motherId: wife1.Id);
        var k2 = TestData.Member(familyId: _familyId, fatherId: husband.Id, motherId: wife2.Id);
        Setup([husband, wife1, wife2, k1, k2]);
        var sut = CreateSut();

        var response = await sut.Handle(new GetFamilyTreeQuery { FamilyId = _familyId }, default);

        response.Data!.Roots.Should().HaveCount(1);
        var root = response.Data.Roots[0];
        // The husband — richest hub by HubScore — is picked as primary.
        root.Primary.Id.Should().Be(husband.Id);
        root.Spouses.Should().HaveCount(2);
        // Each child sits under its own mother's spouse-group.
        root.Spouses.SelectMany(s => s.Children).Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_OrphanedChildOfOutsideFather_FallsBackAsRoot()
    {
        // Father not in this family's loaded set → child becomes a root via the
        // disconnected-fallback pass, otherwise it would never render.
        var orphan = TestData.Member(familyId: _familyId, fatherId: Guid.NewGuid());
        Setup([orphan]);
        var sut = CreateSut();

        var response = await sut.Handle(new GetFamilyTreeQuery { FamilyId = _familyId }, default);

        response.Data!.Roots.Should().HaveCount(1);
        response.Data.Roots[0].Primary.Id.Should().Be(orphan.Id);
    }

    [Fact]
    public async Task Handle_PopulatesProjectionFields()
    {
        var father = TestData.Member(familyId: _familyId, firstName: "F", lastName: "X");
        var mother = TestData.Member(familyId: _familyId, firstName: "M", lastName: "X", gender: Gender.FEMALE);
        var child = TestData.Member(
            familyId: _familyId,
            firstName: "C", lastName: "X",
            fatherId: father.Id, motherId: mother.Id);
        Setup([father, mother, child]);
        var sut = CreateSut();

        var response = await sut.Handle(new GetFamilyTreeQuery { FamilyId = _familyId }, default);

        // Father/Mother names resolve via byId and ride along on the projection
        // (added so the UI tooltip can show them without an extra round-trip).
        var childNode = FlattenChildren(response.Data!.Roots).Single(n => n.Primary.Id == child.Id);
        childNode.Primary.FatherName.Should().Be("F X");
        childNode.Primary.MotherName.Should().Be("M X");
    }

    private static IEnumerable<TreeNodeViewModel> FlattenChildren(IEnumerable<TreeNodeViewModel> roots)
    {
        foreach (var r in roots)
        {
            yield return r;
            foreach (var c in r.CommonChildren)
                foreach (var f in FlattenChildren([c]))
                    yield return f;
            foreach (var g in r.Spouses)
                foreach (var c in g.Children)
                    foreach (var f in FlattenChildren([c]))
                        yield return f;
        }
    }
}
