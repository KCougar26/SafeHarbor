import { useEffect, useMemo, useState, type CSSProperties } from 'react'
import { fetchImpactSummary } from '../services/impactApi'
import type { ImpactSummary } from '../types/impact'

function formatDateTime(iso: string): string {
  return new Intl.DateTimeFormat('en-US', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(iso))
}

export function ImpactDashboardPage() {
  const [data, setData] = useState<ImpactSummary | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadImpact = async () => {
      setLoading(true)
      setError(null)
      try {
        const summary = await fetchImpactSummary()
        setData(summary)
      } catch {
        setError('Unable to load aggregated impact data right now.')
      } finally {
        setLoading(false)
      }
    }

    void loadImpact()
  }, [])

  const outcomeTotal = useMemo(
    () => data?.outcomes.reduce((sum, item) => sum + item.count, 0) ?? 0,
    [data],
  )

  return (
    <section aria-labelledby="impact-title">
      <h1 id="impact-title">Impact Dashboard</h1>
      <p className="lead">
        This dashboard presents anonymized, aggregated outcomes only. No individual profiles
        are exposed.
      </p>

      {loading && <p role="status">Loading impact metrics…</p>}
      {!loading && error && <p role="alert">{error}</p>}

      {!loading && data && (
        <>
          <p className="caption">Last generated: {formatDateTime(data.generatedAt)}</p>

          <section className="metric-grid" aria-label="Key metrics">
            {data.metrics.map((metric) => (
              <article key={metric.label} className="metric-card">
                <h2>{metric.label}</h2>
                <p className="metric-value">{metric.value.toLocaleString()}</p>
                <p className="caption">{metric.changePercent}% vs prior reporting period</p>
              </article>
            ))}
          </section>

          <section className="chart-grid" aria-label="Aggregate charts">
            <article className="chart-card">
              <h2>Monthly households assisted</h2>
              <ul className="bar-list" aria-label="Monthly trend chart">
                {data.monthlyTrend.map((point) => (
                  <li key={point.month}>
                    <span>{point.month}</span>
                    <div
                      className="bar"
                      style={{ '--bar-width': `${point.assistedHouseholds}%` } as CSSProperties}
                      aria-hidden="true"
                    />
                    <span>{point.assistedHouseholds}</span>
                  </li>
                ))}
              </ul>
            </article>

            <article className="chart-card">
              <h2>Outcome distribution</h2>
              <ul className="stack-list" aria-label="Outcome distribution chart">
                {data.outcomes.map((item) => {
                  const percentage = outcomeTotal === 0 ? 0 : (item.count / outcomeTotal) * 100
                  return (
                    <li key={item.category}>
                      <div className="stack-label-row">
                        <span>{item.category}</span>
                        <span>{item.count}</span>
                      </div>
                      <div
                        className="stack"
                        style={{ '--stack-width': `${percentage}%` } as CSSProperties}
                        aria-hidden="true"
                      />
                    </li>
                  )
                })}
              </ul>
            </article>
          </section>
        </>
      )}
    </section>
  )
}
