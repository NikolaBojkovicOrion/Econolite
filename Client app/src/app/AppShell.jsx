import { NavLink, Route, Routes } from 'react-router-dom'

function DashboardPage() {
  return (
    <section className="page">
      <div className="hero-copy">
        <p className="eyebrow">Traffic operations platform</p>
        <h1>See the network clearly.</h1>
        <p>
          A focused workspace for monitoring intersections, understanding detector
          health, and responding to roadway events.
        </p>
      </div>
      <div className="metric-grid">
        <article className="metric-card"><strong>24</strong><span>Intersections monitored</span></article>
        <article className="metric-card"><strong>18</strong><span>Healthy connections</span></article>
        <article className="metric-card"><strong>06</strong><span>Events needing review</span></article>
      </div>
      <div className="placeholder-note">
        Feature 1 foundation is ready. Live intersection data will arrive with the
        persistence feature.
      </div>
    </section>
  )
}

function FeaturePage({ title, description }) {
  return (
    <section className="page">
      <div className="page-header">
        <div>
          <p className="eyebrow">Workspace view</p>
          <h1>{title}</h1>
        </div>
        <p>{description}</p>
      </div>
      <div className="feature-grid">
        <article className="feature-panel">
          <strong>Coming next</strong>
          <h2>Foundation complete</h2>
          <p>This route is ready for its feature-specific API and components.</p>
        </article>
        <article className="feature-panel">
          <strong>Operational principle</strong>
          <h2>Make state visible</h2>
          <p>Loading, empty, stale, error, and disconnected states will be explicit.</p>
        </article>
      </div>
    </section>
  )
}

function AppShell() {
  return (
    <div className="app-shell">
      <header className="topbar">
        <NavLink className="brand" to="/">
          <span>Econolite</span>
          Mobility control room
        </NavLink>
        <nav className="primary-nav" aria-label="Primary navigation">
          <NavLink to="/">Overview</NavLink>
          <NavLink to="/intersections">Intersections</NavLink>
          <NavLink to="/events">Events</NavLink>
          <NavLink to="/audit">Audit</NavLink>
        </nav>
      </header>
      <Routes>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/intersections" element={<FeaturePage title="Intersections" description="Monitor the health and freshness of every roadway connection." />} />
        <Route path="/events" element={<FeaturePage title="Traffic events" description="Review congestion, detector signals, and incidents that need attention." />} />
        <Route path="/audit" element={<FeaturePage title="Audit history" description="Trace important operator and system actions through an accountable record." />} />
        <Route path="*" element={<FeaturePage title="Not found" description="The requested workspace view does not exist." />} />
      </Routes>
    </div>
  )
}

export default AppShell