import type { ImpactSummary } from '../types/impact'

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? ''
const IMPACT_ENDPOINT = import.meta.env.VITE_IMPACT_AGGREGATE_PATH ?? '/api/impact/aggregate'

const fallbackImpactData: ImpactSummary = {
  generatedAt: '2026-04-01T00:00:00.000Z',
  metrics: [
    { label: 'Households Supported', value: 482, changePercent: 8.4 },
    { label: 'Referrals Completed', value: 317, changePercent: 5.2 },
    { label: 'Partner Programs Active', value: 28, changePercent: 12.6 },
  ],
  monthlyTrend: [
    { month: 'Nov', assistedHouseholds: 57 },
    { month: 'Dec', assistedHouseholds: 61 },
    { month: 'Jan', assistedHouseholds: 70 },
    { month: 'Feb', assistedHouseholds: 74 },
    { month: 'Mar', assistedHouseholds: 83 },
  ],
  outcomes: [
    { category: 'Safe Housing', count: 198 },
    { category: 'Medical Stabilization', count: 121 },
    { category: 'Legal Aid', count: 74 },
    { category: 'Employment Transition', count: 89 },
  ],
}

export async function fetchImpactSummary(): Promise<ImpactSummary> {
  try {
    // NOTE: The dashboard must remain read-only and aggregate-only.
    // This client call intentionally requests only rolled-up analytics data.
    const response = await fetch(`${API_BASE}${IMPACT_ENDPOINT}`, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    })

    if (!response.ok) {
      throw new Error(`Impact endpoint returned ${response.status}`)
    }

    const data = (await response.json()) as ImpactSummary
    return data
  } catch {
    // NOTE: Local fallback keeps the UI testable before backend rollout.
    return fallbackImpactData
  }
}
