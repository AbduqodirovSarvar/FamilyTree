using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels.Tree;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MemberEntity = Domain.Entities.Member;

namespace Application.Features.Family.Queries.GetFamilyTree
{
    /// <summary>
    /// Builds a forest of family-tree nodes for a given family.
    ///
    /// A node is a "family hub" — a primary member plus their spouses, with
    /// children grouped per-spouse. Spouses are detected from <c>SpouseId</c>
    /// AND implicitly from being the other parent of any child of the primary,
    /// so a primary with multiple wives renders correctly even though the
    /// schema stores only one canonical <c>SpouseId</c> per member.
    /// </summary>
    public class GetFamilyTreeQueryHandler(
        IMemberRepository memberRepository,
        IFamilyRepository familyRepository
    ) : IRequestHandler<GetFamilyTreeQuery, Response<FamilyTreeViewModel>>
    {
        private readonly IMemberRepository _memberRepository = memberRepository;
        private readonly IFamilyRepository _familyRepository = familyRepository;

        public async Task<Response<FamilyTreeViewModel>> Handle(GetFamilyTreeQuery request, CancellationToken cancellationToken)
        {
            if (request.FamilyId == Guid.Empty)
                return Response<FamilyTreeViewModel>.Fail("FamilyId is required.");

            var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
                ?? throw new KeyNotFoundException("Family not found.");

            var members = await _memberRepository.GetAllAsync(
                m => m.FamilyId == request.FamilyId,
                cancellationToken);

            var roots = BuildForest(members);

            var result = new FamilyTreeViewModel
            {
                FamilyId = family.Id,
                Name = family.Name,
                FamilyName = family.FamilyName,
                TotalMembers = members.Count,
                Roots = roots
            };

            return Response<FamilyTreeViewModel>.Ok(result);
        }

        private static List<TreeNodeViewModel> BuildForest(List<MemberEntity> members)
        {
            var byId = members.ToDictionary(m => m.Id);
            var visited = new HashSet<Guid>();

            bool InFamily(Guid? id) => id.HasValue && byId.ContainsKey(id.Value);

            Guid? PrimaryParentId(MemberEntity m)
            {
                if (InFamily(m.FatherId)) return m.FatherId;
                if (InFamily(m.MotherId)) return m.MotherId;
                return null;
            }

            List<MemberEntity> ChildrenOf(Guid id) =>
                members.Where(x => x.FatherId == id || x.MotherId == id).ToList();

            Guid? OtherParentOf(MemberEntity child, Guid memberId)
            {
                if (child.FatherId == memberId) return child.MotherId;
                if (child.MotherId == memberId) return child.FatherId;
                return null;
            }

            TreeNodeViewModel? BuildNode(Guid memberId)
            {
                if (visited.Contains(memberId)) return null;
                if (!byId.TryGetValue(memberId, out var primary)) return null;
                visited.Add(memberId);

                var spouseIds = new HashSet<Guid>();
                if (InFamily(primary.SpouseId)) spouseIds.Add(primary.SpouseId!.Value);

                var childrenOfPrimary = ChildrenOf(primary.Id);
                foreach (var c in childrenOfPrimary)
                {
                    var otherId = OtherParentOf(c, primary.Id);
                    if (otherId.HasValue && InFamily(otherId)) spouseIds.Add(otherId.Value);
                }

                foreach (var sid in spouseIds) visited.Add(sid);

                var spouseGroups = new List<SpouseGroupViewModel>();
                foreach (var sid in spouseIds)
                {
                    var spouse = byId[sid];
                    var groupKids = childrenOfPrimary
                        .Where(c => OtherParentOf(c, primary.Id) == sid)
                        .ToList();

                    spouseGroups.Add(new SpouseGroupViewModel
                    {
                        Spouse = ToTreeMember(spouse),
                        Children = groupKids
                            .Select(c => BuildNode(c.Id))
                            .Where(n => n != null)
                            .Cast<TreeNodeViewModel>()
                            .ToList()
                    });
                }

                var commonChildren = childrenOfPrimary
                    .Where(c =>
                    {
                        var otherId = OtherParentOf(c, primary.Id);
                        return !otherId.HasValue || !InFamily(otherId);
                    })
                    .Select(c => BuildNode(c.Id))
                    .Where(n => n != null)
                    .Cast<TreeNodeViewModel>()
                    .ToList();

                return new TreeNodeViewModel
                {
                    Primary = ToTreeMember(primary),
                    Spouses = spouseGroups,
                    CommonChildren = commonChildren
                };
            }

            // Roots: members with no in-family parent. Husband (gender=0) preferred
            // over wife (gender=1) when both qualify, so couple is rendered once.
            var sorted = members
                .OrderBy(m => PrimaryParentId(m).HasValue ? 1 : 0)
                .ThenBy(m => (int)m.Gender)
                .ToList();

            var roots = new List<TreeNodeViewModel>();
            foreach (var m in sorted)
            {
                if (visited.Contains(m.Id)) continue;
                if (PrimaryParentId(m).HasValue) continue;
                var node = BuildNode(m.Id);
                if (node != null) roots.Add(node);
            }

            // Pick up any disconnected members (cycles, orphan refs).
            foreach (var m in members)
            {
                if (visited.Contains(m.Id)) continue;
                var node = BuildNode(m.Id);
                if (node != null) roots.Add(node);
            }

            // Largest/deepest sub-tree first — so the "main" family shows at the top
            // of the canvas and standalone childless couples render below.
            return roots
                .OrderByDescending(NodeSize)
                .ThenByDescending(NodeDepth)
                .ToList();
        }

        private static int NodeSize(TreeNodeViewModel node)
        {
            int count = 1 + node.Spouses.Count;
            foreach (var c in node.CommonChildren) count += NodeSize(c);
            foreach (var g in node.Spouses)
                foreach (var c in g.Children) count += NodeSize(c);
            return count;
        }

        private static int NodeDepth(TreeNodeViewModel node)
        {
            int childMax = 0;
            foreach (var c in node.CommonChildren) childMax = Math.Max(childMax, NodeDepth(c));
            foreach (var g in node.Spouses)
                foreach (var c in g.Children) childMax = Math.Max(childMax, NodeDepth(c));
            return 1 + childMax;
        }

        private static TreeMemberViewModel ToTreeMember(MemberEntity m) => new()
        {
            Id = m.Id,
            FirstName = m.FirstName,
            LastName = m.LastName,
            Gender = m.Gender,
            BirthDay = m.BirthDay,
            DeathDay = m.DeathDay,
            ImageId = m.ImageId,
            ImageUrl = m.Image?.Url
        };
    }
}
