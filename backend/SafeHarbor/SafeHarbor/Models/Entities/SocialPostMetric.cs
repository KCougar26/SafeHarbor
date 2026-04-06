namespace SafeHarbor.Models.Entities;

public class SocialPostMetric : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid? CampaignId { get; set; }
    public DateTimeOffset PostedAt { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int Reach { get; set; }
    public int Engagements { get; set; }

    // NOTE: Attribution values remain nullable because many platforms do not provide
    // deterministic conversion attribution per post.
    public decimal? AttributedDonationAmount { get; set; }
    public int? AttributedDonationCount { get; set; }

    public Campaign? Campaign { get; set; }
}
