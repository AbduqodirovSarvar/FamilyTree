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
    /// children grouped per-spouse. Spouses are detected four ways so the tree
    /// stays connected even when data is partial:
    ///   1. <c>primary.SpouseId</c> (forward)
    ///   2. another member with <c>SpouseId == primary.Id</c> (reverse — handles
    ///      one-sided spouse links, which previously rendered the couple as two
    ///      disconnected hubs)
    ///   3. implicit via shared child (the "other parent" of any of primary's
    ///      children who is in the same family)
    ///   4. inferred "kelin" pairing — when a member has no relations of any
    ///      kind (no parent / no spouse / no child in family) and there is
    ///      exactly one opposite-gender "frontier" member (has parents in family
    ///      but no spouse and no kids), pair them. Common pattern: user adds a
    ///      daughter-in-law without explicit links and expects her to land next
    ///      to the obvious husband candidate instead of as a disconnected root.
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

            // Local helper so the projection can resolve names from byId without
            // shipping the whole graph to the client. Returns null when the
            // referenced person isn't part of this family's loaded set.
            string? FormatName(Guid? id)
            {
                if (!id.HasValue || !byId.TryGetValue(id.Value, out var p)) return null;
                var combined = $"{p.FirstName} {p.LastName}".Trim();
                return string.IsNullOrEmpty(combined) ? null : combined;
            }

            TreeMemberViewModel ToTreeMember(MemberEntity m) => new()
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Description = m.Description,
                Gender = m.Gender,
                BirthDay = m.BirthDay,
                DeathDay = m.DeathDay,
                ImageId = m.ImageId,
                ImageUrl = m.Image?.Url,
                FatherName = FormatName(m.FatherId),
                MotherName = FormatName(m.MotherId),
                SpouseName = FormatName(m.SpouseId)
            };

            bool InFamily(Guid? id) => id.HasValue && byId.ContainsKey(id.Value);

            Guid? PrimaryParentId(MemberEntity m)
            {
                if (InFamily(m.FatherId)) return m.FatherId;
                if (InFamily(m.MotherId)) return m.MotherId;
                return null;
            }

            // Index of children per parent — single pass instead of repeated scans.
            var childrenByParent = new Dictionary<Guid, List<MemberEntity>>();
            foreach (var m in members)
            {
                if (m.FatherId.HasValue && InFamily(m.FatherId))
                    AddChild(childrenByParent, m.FatherId!.Value, m);
                if (m.MotherId.HasValue && InFamily(m.MotherId))
                    AddChild(childrenByParent, m.MotherId!.Value, m);
            }

            // Birinchi farzand chapda tursin: oldest → youngest. DB-dan kelgan
            // tartib aralash (oldin kichik uka, keyin katta aka) bo'lib chiqadi
            // va bu tree-da chalkash ko'rinadi.
            foreach (var list in childrenByParent.Values)
                list.Sort((a, b) => a.BirthDay.CompareTo(b.BirthDay));

            List<MemberEntity> ChildrenOf(Guid id) =>
                childrenByParent.TryGetValue(id, out var list) ? list : new List<MemberEntity>();

            Guid? OtherParentOf(MemberEntity child, Guid memberId)
            {
                if (child.FatherId == memberId) return child.MotherId;
                if (child.MotherId == memberId) return child.FatherId;
                return null;
            }

            // Reverse spouse index — every member that someone else points to via SpouseId.
            // Used both to detect spouses bidirectionally and to skip these from root pass.
            var spouseReverse = new Dictionary<Guid, List<Guid>>();
            foreach (var m in members)
            {
                if (!InFamily(m.SpouseId)) continue;
                if (!spouseReverse.TryGetValue(m.SpouseId!.Value, out var list))
                    spouseReverse[m.SpouseId!.Value] = list = new List<Guid>();
                list.Add(m.Id);
            }

            // ─────────────────────────────────────────────────────────────────
            // Inferred "kelin" pairings — bridges the common UX gap where a
            // user adds a daughter-in-law without setting any explicit links
            // (no parents, no spouseId, no children) and expects her to land
            // next to the obvious husband instead of in a disconnected corner.
            //
            // Rule: a "stray" (no parent / no explicit spouse / no children in
            // family) is paired with the unique opposite-gender "frontier"
            // member (has parents in family but no explicit spouse and no
            // children). If multiple candidates exist on either side, we skip
            // the pairing — guessing among siblings is too risky.
            // ─────────────────────────────────────────────────────────────────
            bool HasExplicitSpouse(MemberEntity m) =>
                m.SpouseId.HasValue || spouseReverse.ContainsKey(m.Id);

            bool HasChildrenInFamily(MemberEntity m) =>
                childrenByParent.ContainsKey(m.Id);

            bool HasParentInFamily(MemberEntity m) =>
                InFamily(m.FatherId) || InFamily(m.MotherId);

            var strays = members
                .Where(m => !HasParentInFamily(m) && !HasExplicitSpouse(m) && !HasChildrenInFamily(m))
                .ToList();

            var frontier = members
                .Where(m => HasParentInFamily(m) && !HasExplicitSpouse(m) && !HasChildrenInFamily(m))
                .ToList();

            var inferredSpouse = new Dictionary<Guid, Guid>();
            foreach (var s in strays)
            {
                var candidates = frontier.Where(f => f.Gender != s.Gender).ToList();
                if (candidates.Count != 1) continue;
                var partner = candidates[0];
                inferredSpouse[s.Id] = partner.Id;
                inferredSpouse[partner.Id] = s.Id;
                frontier.Remove(partner);
            }

            HashSet<Guid> SpousesOf(MemberEntity primary)
            {
                var ids = new HashSet<Guid>();

                if (InFamily(primary.SpouseId)) ids.Add(primary.SpouseId!.Value);

                if (spouseReverse.TryGetValue(primary.Id, out var reverseRefs))
                    foreach (var rid in reverseRefs) ids.Add(rid);

                foreach (var c in ChildrenOf(primary.Id))
                {
                    var otherId = OtherParentOf(c, primary.Id);
                    if (otherId.HasValue && InFamily(otherId)) ids.Add(otherId.Value);
                }

                if (inferredSpouse.TryGetValue(primary.Id, out var inferredId))
                    ids.Add(inferredId);

                ids.Remove(primary.Id);
                return ids;
            }

            TreeNodeViewModel? BuildNode(Guid memberId)
            {
                if (visited.Contains(memberId)) return null;
                if (!byId.TryGetValue(memberId, out var primary)) return null;
                visited.Add(memberId);

                var spouseIds = SpousesOf(primary);
                foreach (var sid in spouseIds) visited.Add(sid);

                var childrenOfPrimary = ChildrenOf(primary.Id);

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

            // Total subtree size (children + grandchildren + ...). Memoized so
            // repeated lookups during sort stay O(1). Cycle-safe via the
            // pre-insert of 0 before recursion. Used in HubScore so a true
            // ancestor (1 child but a deep tree) outranks a daughter-in-law
            // sitting one generation below (many direct children but no tree
            // above her).
            var descendantCache = new Dictionary<Guid, int>();
            int DescendantCount(Guid memberId)
            {
                if (descendantCache.TryGetValue(memberId, out var cached))
                    return cached;
                descendantCache[memberId] = 0;
                int count = 0;
                foreach (var child in ChildrenOf(memberId))
                    count += 1 + DescendantCount(child.Id);
                descendantCache[memberId] = count;
                return count;
            }

            // "Married-in" detector. A founder ancestor's spouse is also from
            // outside (no parents in family). A kelin/kuyov's spouse, by
            // contrast, has parents in family — that's the blood-line link.
            // We use this to demote in-laws among root candidates so the
            // actual head of family surfaces as primary even when the in-law
            // happens to have more direct children.
            bool SpouseHasParentInFamily(MemberEntity m)
            {
                foreach (var sid in SpousesOf(m))
                {
                    if (byId.TryGetValue(sid, out var spouse) && HasParentInFamily(spouse))
                        return true;
                }
                return false;
            }

            // Hub score: how rich is this member's full subtree if we make
            // them the primary? Weighted by total descendant count, then
            // spouse count, then a MALE-first cultural tiebreaker. Total
            // descendants (vs. direct children) is what picks bobo (3 wives,
            // 2 kids) over each individual wife as the root for a polygamous
            // family — and equally what picks the eldest ancestor over their
            // daughter-in-law when she has more direct children but a
            // shallower tree.
            int HubScore(MemberEntity m)
            {
                int spouseCount = SpousesOf(m).Count;
                int subtreeSize = DescendantCount(m.Id);
                return subtreeSize * 100
                       + spouseCount * 10
                       + (m.Gender == Domain.Enums.Gender.MALE ? 1 : 0);
            }

            // Roots: members with no in-family parent. Within that, prefer
            // blood ancestors over married-in spouses (whose partner DOES
            // have parents in family), then the richest hub. Without the
            // married-in tier, an in-law with many direct children can
            // outrank the founder ancestor and leave the founder as a
            // separate root with an empty hub (everyone below is already
            // claimed by the in-law's sub-tree).
            var sorted = members
                .OrderBy(m => PrimaryParentId(m).HasValue ? 1 : 0)
                .ThenBy(m => SpouseHasParentInFamily(m) ? 1 : 0)
                .ThenByDescending(HubScore)
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

        private static void AddChild(Dictionary<Guid, List<MemberEntity>> map, Guid parentId, MemberEntity child)
        {
            if (!map.TryGetValue(parentId, out var list))
                map[parentId] = list = new List<MemberEntity>();
            if (!list.Contains(child)) list.Add(child);
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

    }
}
