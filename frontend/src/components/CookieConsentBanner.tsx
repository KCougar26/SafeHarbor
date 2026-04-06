import { useEffect, useState } from 'react'

type ConsentState = {
  necessary: true
  analytics: boolean
  preferences: boolean
}

const STORAGE_KEY = 'safeharbor-cookie-consent'

const defaultConsent: ConsentState = {
  necessary: true,
  analytics: false,
  preferences: false,
}

function loadStoredConsent(): ConsentState | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as ConsentState
  } catch {
    return null
  }
}

export function CookieConsentBanner() {
  const [consent, setConsent] = useState<ConsentState>(defaultConsent)
  const [isVisible, setIsVisible] = useState(false)
  const [showSettings, setShowSettings] = useState(false)

  useEffect(() => {
    const storedConsent = loadStoredConsent()
    if (storedConsent) {
      setConsent(storedConsent)
      setIsVisible(false)
      return
    }

    setIsVisible(true)
  }, [])

  const persistConsent = (nextConsent: ConsentState) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(nextConsent))
    setConsent(nextConsent)
    setIsVisible(false)
    setShowSettings(false)
  }

  if (!isVisible && !showSettings) {
    return null
  }

  return (
    <>
      {isVisible && (
        <aside className="cookie-banner" aria-label="Cookie consent">
          <div>
            <strong>Cookie preferences</strong>
            <p>
              We use necessary cookies for security, and optional cookies for analytics and
              personalization controls.
            </p>
          </div>
          <div className="cookie-actions">
            <button
              type="button"
              className="button button-secondary"
              onClick={() => setShowSettings(true)}
            >
              Manage preferences
            </button>
            <button
              type="button"
              className="button button-primary"
              onClick={() => persistConsent({ ...consent, analytics: true, preferences: true })}
            >
              Accept all
            </button>
          </div>
        </aside>
      )}

      {showSettings && (
        <div className="modal-backdrop" role="presentation">
          <section
            className="modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="cookie-settings-title"
          >
            <h2 id="cookie-settings-title">Cookie settings</h2>
            <p className="caption">Necessary cookies are always enabled for security reasons.</p>

            <label className="consent-row">
              <input type="checkbox" checked disabled />
              <span>Necessary cookies (required)</span>
            </label>

            <label className="consent-row">
              <input
                type="checkbox"
                checked={consent.analytics}
                onChange={(event) =>
                  setConsent((previous) => ({ ...previous, analytics: event.target.checked }))
                }
              />
              <span>Analytics cookies</span>
            </label>

            <label className="consent-row">
              <input
                type="checkbox"
                checked={consent.preferences}
                onChange={(event) =>
                  setConsent((previous) => ({ ...previous, preferences: event.target.checked }))
                }
              />
              <span>Preference cookies</span>
            </label>

            <div className="cookie-actions">
              <button
                type="button"
                className="button button-secondary"
                onClick={() => {
                  setConsent(defaultConsent)
                  persistConsent(defaultConsent)
                }}
              >
                Reject optional
              </button>
              <button
                type="button"
                className="button button-primary"
                onClick={() => persistConsent(consent)}
              >
                Save preferences
              </button>
            </div>
          </section>
        </div>
      )}
    </>
  )
}
