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
