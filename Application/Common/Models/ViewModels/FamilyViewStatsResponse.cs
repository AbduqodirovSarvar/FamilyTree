namespace Application.Common.Models.ViewModels
{
    public sealed record FamilyViewStatsPoint(string Date, int Count);

    public sealed record FamilyViewStatsResponse(
        Guid FamilyId,
        string FamilyName,
        string From,
        string To,
        int Total,
        IReadOnlyList<FamilyViewStatsPoint> Points);
}
