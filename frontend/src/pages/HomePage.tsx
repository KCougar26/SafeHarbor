import { Link } from 'react-router-dom'

const missionPoints = [
  'Protect survivor privacy with anonymized reporting and role-based access.',
  'Coordinate services across housing, healthcare, legal aid, and community partners.',
  'Measure impact transparently while safeguarding sensitive records.',
]

export function HomePage() {
  return (
    <section aria-labelledby="home-title">
      <div className="hero-card">
        <p className="eyebrow">Safe Harbor Platform</p>
        <h1 id="home-title">Support informed, privacy-first intervention at scale.</h1>
        <p className="lead">
          Safe Harbor helps partner organizations coordinate care pathways while keeping
          person-level details protected.
        </p>
        <div className="cta-row">
          <Link to="/impact" className="button button-primary">
            View impact dashboard
          </Link>
          <Link to="/login" className="button button-secondary">
            Team login
          </Link>
        </div>
      </div>

      <div className="feature-grid" aria-label="Mission highlights">
        {missionPoints.map((point) => (
          <article key={point} className="feature-card">
            <h2>Mission focus</h2>
            <p>{point}</p>
          </article>
        ))}
      </div>
    </section>
  )
}
