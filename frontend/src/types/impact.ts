export type ImpactMetric = {
  label: string
  value: number
  changePercent: number
}

export type MonthlyTrendPoint = {
  month: string
  assistedHouseholds: number
}

export type OutcomeDistribution = {
  category: string
  count: number
}

export type ImpactSummary = {
  generatedAt: string
  metrics: ImpactMetric[]
  monthlyTrend: MonthlyTrendPoint[]
  outcomes: OutcomeDistribution[]
}

export type DonationTrendPoint = {
  month: string
  amount: number
}

export type OutcomeTrendPoint = {
  month: string
  residentsServed: number
  homeVisits: number
}

export type SafehouseComparisonItem = {
  safehouse: string
  activeResidents: number
  allocatedFunding: number
}

export type ReintegrationRatePoint = {
  month: string
  ratePercent: number
}

export type SocialDonationCorrelationPoint = {
  group: string
  posts: number
  totalReach: number
  totalEngagements: number
  totalAttributedDonationAmount: number
  totalAttributedDonationCount: number
  donationsPer1kReach: number
  engagementRatePercent: number
}

export type SocialPostDonationInsight = {
  postMetricId: string
  postedAt: string
  platform: string
  contentType: string
  reach: number
  engagements: number
  attributedDonationAmount: number | null
  attributedDonationCount: number | null
  engagementRatePercent: number
}

export type ContentTimingRecommendationCard = {
  title: string
  rationale: string
  action: string
}

export type ReportsAnalyticsResponse = {
  donationTrends: DonationTrendPoint[]
  outcomeTrends: OutcomeTrendPoint[]
  safehouseComparisons: SafehouseComparisonItem[]
  reintegrationRates: ReintegrationRatePoint[]
  donationCorrelationByPlatform: SocialDonationCorrelationPoint[]
  donationCorrelationByContentType: SocialDonationCorrelationPoint[]
  donationCorrelationByPostingHour: SocialDonationCorrelationPoint[]
  topAttributedPosts: SocialPostDonationInsight[]
  recommendations: ContentTimingRecommendationCard[]
}

export type CreateSocialPostMetricRequest = {
  campaignId?: string | null
  postedAt: string
  platform: string
  contentType: string
  reach: number
  engagements: number
  attributedDonationAmount?: number | null
  attributedDonationCount?: number | null
}

export type SocialPostMetricListItem = {
  id: string
  campaignId: string | null
  postedAt: string
  platform: string
  contentType: string
  reach: number
  engagements: number
  attributedDonationAmount: number | null
  attributedDonationCount: number | null
}
