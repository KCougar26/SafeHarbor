import { NavLink, Outlet } from 'react-router-dom'
import { CookieConsentBanner } from './components/CookieConsentBanner'

const navigation = [
  { to: '/', label: 'Home' },
  { to: '/impact', label: 'Impact Dashboard' },
  { to: '/login', label: 'Login' },
  { to: '/privacy', label: 'Privacy' },
]

function App() {
  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content">
        Skip to main content
      </a>

      <header className="site-header" role="banner">
        <div className="container nav-container">
          <div className="brand" aria-label="Safe Harbor">
            Safe Harbor
          </div>
          <nav aria-label="Primary">
            <ul className="nav-list">
              {navigation.map((item) => (
                <li key={item.to}>
                  <NavLink
                    to={item.to}
                    className={({ isActive }) =>
                      `nav-link${isActive ? ' nav-link-active' : ''}`
                    }
                    end={item.to === '/'}
                  >
                    {item.label}
                  </NavLink>
                </li>
              ))}
            </ul>
          </nav>
        </div>
      </header>

      <main id="main-content" className="container page-content" role="main">
        <Outlet />
      </main>

      <footer className="site-footer" role="contentinfo">
        <div className="container footer-content">
          <p>© {new Date().getFullYear()} Safe Harbor</p>
          <p>Anonymized impact insights for responsible care partnerships.</p>
        </div>
      </footer>

      <CookieConsentBanner />
    </div>
  )
}

export default App
