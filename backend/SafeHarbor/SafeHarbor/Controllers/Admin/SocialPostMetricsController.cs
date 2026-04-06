using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeHarbor.Data;
using SafeHarbor.DTOs;
using SafeHarbor.Models.Entities;

namespace SafeHarbor.Controllers.Admin;

[ApiController]
[Route("api/admin/social-post-metrics")]
[Authorize]
public sealed class SocialPostMetricsController(SafeHarborDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SocialPostMetricListItem>>> Get(CancellationToken cancellationToken)
    {
        var items = await dbContext.SocialPostMetrics
            .AsNoTracking()
            .OrderByDescending(x => x.PostedAt)
            .Select(x => new SocialPostMetricListItem(
                x.Id,
                x.CampaignId,
                x.PostedAt,
                x.Platform,
                x.ContentType,
                x.Reach,
                x.Engagements,
                x.AttributedDonationAmount,
                x.AttributedDonationCount))
            .ToArrayAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<SocialPostMetricListItem>> Create([FromBody] CreateSocialPostMetricRequest request, CancellationToken cancellationToken)
    {
        // NOTE: Correlation reporting relies on consistent grouping keys.
        // Inputs are normalized to avoid duplicate buckets caused by casing/whitespace.
        var metric = new SocialPostMetric
        {
            Id = Guid.NewGuid(),
            CampaignId = request.CampaignId,
            PostedAt = request.PostedAt,
            Platform = request.Platform.Trim(),
            ContentType = request.ContentType.Trim(),
            Reach = request.Reach,
            Engagements = request.Engagements,
            AttributedDonationAmount = request.AttributedDonationAmount,
            AttributedDonationCount = request.AttributedDonationCount
        };

        dbContext.SocialPostMetrics.Add(metric);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { metric.Id }, new SocialPostMetricListItem(
            metric.Id,
            metric.CampaignId,
            metric.PostedAt,
            metric.Platform,
            metric.ContentType,
            metric.Reach,
            metric.Engagements,
            metric.AttributedDonationAmount,
            metric.AttributedDonationCount));
    }
}
