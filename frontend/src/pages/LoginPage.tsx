import { useState, type FormEvent } from 'react'

export function LoginPage() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)

  const handleCredentialsSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (!username.trim() || !password.trim()) {
      setError('Please enter both username and password.')
      return
    }

    // NOTE: The authentication backend is not wired in this sprint.
    // We keep this UX state explicit so product review can validate failures.
    setError('Login is currently unavailable. Use your Entra SSO flow when enabled.')
  }

  const handleEntraRedirect = () => {
    // NOTE: This placeholder mirrors the eventual redirect button behavior.
    setError('Microsoft Entra redirect is not configured in this environment.')
  }

  return (
    <section aria-labelledby="login-title" className="auth-layout">
      <div className="auth-card">
        <h1 id="login-title">Team login</h1>
        <p className="caption">Use your credentials or single sign-on with Microsoft Entra.</p>

        {error && (
          <p className="form-error" role="alert">
            {error}
          </p>
        )}

        <form onSubmit={handleCredentialsSubmit} className="auth-form">
          <label htmlFor="username">Username</label>
          <input
            id="username"
            name="username"
            autoComplete="username"
            value={username}
            onChange={(event) => setUsername(event.target.value)}
          />

          <label htmlFor="password">Password</label>
          <input
            id="password"
            name="password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />

          <button type="submit" className="button button-primary">
            Continue with credentials
          </button>
        </form>

        <div className="divider" aria-hidden="true">
          or
        </div>

        <button type="button" className="button button-secondary" onClick={handleEntraRedirect}>
          Continue with Microsoft Entra
        </button>
      </div>
    </section>
  )
}
