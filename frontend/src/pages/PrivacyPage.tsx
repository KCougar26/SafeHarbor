const controls = [
  {
    title: 'Necessary cookies',
    detail: 'Required for security, session management, and accessibility preferences.',
  },
  {
    title: 'Analytics cookies',
    detail: 'Measure aggregate product usage to improve service quality.',
  },
  {
    title: 'Preference cookies',
    detail: 'Store non-sensitive preferences like theme and language selection.',
  },
]

export function PrivacyPage() {
  return (
    <section aria-labelledby="privacy-title">
      <h1 id="privacy-title">Privacy policy</h1>
      <p className="lead">
        Safe Harbor collects the minimum data required to coordinate care and report
        anonymized outcomes. Sensitive records are protected with strict access controls.
      </p>

      <article className="policy-card">
        <h2>How we process data</h2>
        <ul>
          <li>Data minimization: only purpose-limited fields are captured.</li>
          <li>Role-based access: access is restricted by verified team role.</li>
          <li>Retention controls: records are retained only as long as required.</li>
          <li>Aggregated reporting: public dashboards never include personal identifiers.</li>
        </ul>
      </article>

      <article className="policy-card">
        <h2>Cookie controls (GDPR style)</h2>
        <p>
          You can opt-in or opt-out of non-essential cookies at any time through the cookie
          preference controls in the footer banner.
        </p>
        <div className="feature-grid">
          {controls.map((control) => (
            <div key={control.title} className="feature-card">
              <h3>{control.title}</h3>
              <p>{control.detail}</p>
            </div>
          ))}
        </div>
      </article>
    </section>
  )
}
