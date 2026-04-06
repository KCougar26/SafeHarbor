using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeHarbor.Data;
using SafeHarbor.DTOs;
using SafeHarbor.Models.Entities;
using SafeHarbor.Models.Enums;

namespace SafeHarbor.Controllers.Admin;

[ApiController]
[Route("api/admin/reports-analytics")]
[Authorize]
public sealed class ReportsAnalyticsController(SafeHarborDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ReportsAnalyticsResponse>> Get(CancellationToken cancellationToken)
    {
        var donationTrends = await dbContext.Contributions
            .AsNoTracking()
            .GroupBy(x => new { x.ContributionDate.Year, x.ContributionDate.Month })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .Select(x => new DonationTrendPoint($"{x.Key.Year}-{x.Key.Month:D2}", x.Sum(y => y.Amount)))
            .ToArrayAsync(cancellationToken);

        var outcomeTrends = await dbContext.OutcomeSnapshots
            .AsNoTracking()
            .OrderBy(x => x.SnapshotDate)
            .Select(x => new OutcomeTrendPoint(
                $"{x.SnapshotDate.Year}-{x.SnapshotDate.Month:D2}",
                x.TotalResidentsServed,
                x.TotalHomeVisits))
            .ToArrayAsync(cancellationToken);

        var activeResidentsBySafehouse = await dbContext.ResidentCases
            .AsNoTracking()
            .Where(x => x.StatusState != null && x.StatusState.Domain == StatusDomain.ResidentCase && (x.StatusState.Code == "OPEN" || x.StatusState.Code == "ACTIVE"))
            .GroupBy(x => x.SafehouseId)
            .Select(x => new { SafehouseId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.SafehouseId, x => x.Count, cancellationToken);

        var allocationBySafehouse = await dbContext.ContributionAllocations
            .AsNoTracking()
            .GroupBy(x => x.SafehouseId)
            .Select(x => new { SafehouseId = x.Key, Amount = x.Sum(y => y.AmountAllocated) })
            .ToDictionaryAsync(x => x.SafehouseId, x => x.Amount, cancellationToken);

        var safehouseComparisons = await dbContext.Safehouses
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SafehouseComparisonItem(
                x.Name,
                activeResidentsBySafehouse.TryGetValue(x.Id, out var activeResidents) ? activeResidents : 0,
                allocationBySafehouse.TryGetValue(x.Id, out var allocatedFunding) ? allocatedFunding : 0))
            .ToArrayAsync(cancellationToken);

        var reintegrationRates = await dbContext.ResidentCases
            .AsNoTracking()
            .Where(x => x.ClosedAt != null)
            .GroupBy(x => new { Year = x.ClosedAt!.Value.Year, Month = x.ClosedAt!.Value.Month })
            .Select(x => new
            {
                x.Key.Year,
                x.Key.Month,
                Closed = x.Count(),
                Opened = dbContext.ResidentCases.Count(y => y.OpenedAt.Year == x.Key.Year && y.OpenedAt.Month == x.Key.Month)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToArrayAsync(cancellationToken);

        var reintegrationRateItems = reintegrationRates
            .Select(x => new ReintegrationRatePoint($"{x.Year}-{x.Month:D2}", x.Closed / (decimal)Math.Max(1, x.Opened) * 100))
            .ToArray();

        var postMetrics = await dbContext.SocialPostMetrics
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        var donationCorrelationByPlatform = BuildCorrelation(postMetrics, x => x.Platform);
        var donationCorrelationByContentType = BuildCorrelation(postMetrics, x => x.ContentType);
        var donationCorrelationByPostingHour = BuildCorrelation(postMetrics, x => x.PostedAt.ToString("HH:00"));

        var topAttributedPosts = postMetrics
            .OrderByDescending(x => x.AttributedDonationAmount ?? 0)
            .ThenByDescending(x => x.Engagements)
            .Take(5)
            .Select(x => new SocialPostDonationInsight(
                x.Id,
                x.PostedAt,
                x.Platform,
                x.ContentType,
                x.Reach,
                x.Engagements,
                x.AttributedDonationAmount,
                x.AttributedDonationCount,
                x.Engagements / (decimal)Math.Max(1, x.Reach) * 100))
            .ToArray();

        var recommendations = BuildRecommendations(donationCorrelationByPlatform, donationCorrelationByContentType, donationCorrelationByPostingHour)
            .ToArray();

        return Ok(new ReportsAnalyticsResponse(
            donationTrends,
            outcomeTrends,
            safehouseComparisons,
            reintegrationRateItems,
            donationCorrelationByPlatform,
            donationCorrelationByContentType,
            donationCorrelationByPostingHour,
            topAttributedPosts,
            recommendations));
    }

    private static SocialDonationCorrelationPoint[] BuildCorrelation(IEnumerable<SocialPostMetric> posts, Func<SocialPostMetric, string> groupingKey)
    {
        return posts
            .GroupBy(groupingKey)
            .Select(group =>
            {
                var totalReach = group.Sum(x => x.Reach);
                var totalEngagements = group.Sum(x => x.Engagements);
                var totalAttributedAmount = group.Sum(x => x.AttributedDonationAmount ?? 0m);
                var totalAttributedCount = group.Sum(x => x.AttributedDonationCount ?? 0);

                return new SocialDonationCorrelationPoint(
                    group.Key,
                    group.Count(),
                    totalReach,
                    totalEngagements,
                    totalAttributedAmount,
                    totalAttributedCount,
                    totalAttributedAmount / Math.Max(1, totalReach) * 1000,
                    totalEngagements / (decimal)Math.Max(1, totalReach) * 100);
            })
            .OrderByDescending(x => x.DonationsPer1kReach)
            .ThenByDescending(x => x.TotalAttributedDonationAmount)
            .ToArray();
    }

    private static IEnumerable<ContentTimingRecommendationCard> BuildRecommendations(
        IReadOnlyCollection<SocialDonationCorrelationPoint> platformCorrelation,
        IReadOnlyCollection<SocialDonationCorrelationPoint> contentTypeCorrelation,
        IReadOnlyCollection<SocialDonationCorrelationPoint> postingHourCorrelation)
    {
        var recommendations = new List<ContentTimingRecommendationCard>();

        var bestPlatform = platformCorrelation.FirstOrDefault();
        if (bestPlatform is not null)
        {
            recommendations.Add(new ContentTimingRecommendationCard(
                "Lean into top donation platform",
                $"{bestPlatform.Group} currently leads with {bestPlatform.DonationsPer1kReach:F2} attributed dollars per 1K reach.",
                $"Schedule at least two additional weekly posts on {bestPlatform.Group} and mirror the highest-performing creative format."));
        }

        var bestContentType = contentTypeCorrelation.FirstOrDefault();
        if (bestContentType is not null)
        {
            recommendations.Add(new ContentTimingRecommendationCard(
                "Adjust content mix toward strongest format",
                $"{bestContentType.Group} posts are producing the highest attributed donation return.",
                $"Target a {Math.Min(70, Math.Max(35, bestContentType.Posts * 10))}% share of {bestContentType.Group} content in the next publishing cycle."));
        }

        var bestHour = postingHourCorrelation.FirstOrDefault();
        if (bestHour is not null)
        {
            recommendations.Add(new ContentTimingRecommendationCard(
                "Shift publishing window",
                $"Posts around {bestHour.Group} show the strongest donation conversion per reach.",
                $"Queue campaign posts between {bestHour.Group} and {NextHour(bestHour.Group)} for the next two weeks, then re-check this report."));
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add(new ContentTimingRecommendationCard(
                "Add attribution-ready social post metrics",
                "No social post metrics are available yet, so recommendations cannot be data-driven.",
                "Start logging platform, content type, reach, engagement, and donation attribution for each post."));
        }

        return recommendations;
    }

    private static string NextHour(string hourLabel)
    {
        if (!int.TryParse(hourLabel[..2], out var hour))
        {
            return "next hour";
        }

        return $"{(hour + 1) % 24:D2}:00";
    }
}
