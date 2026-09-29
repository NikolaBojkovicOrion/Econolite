import { ActiveEventList } from '../../trafficEvents/components/ActiveEventList'
import { IntersectionStatusList } from '../../intersections/components/IntersectionStatusList'
import { MetricCard } from '../components/MetricCard'
import { useDashboardData } from '../useDashboardData'

export function DashboardPage() {
  const { dashboard, retry } = useDashboardData()
  const healthyCount = dashboard.intersections.filter(
    (intersection) => intersection.status === 'Healthy',
  ).length

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
        <MetricCard
          value={dashboard.status === 'ready' ? dashboard.intersections.length : '—'}
          label="Intersections monitored"
        />
        <MetricCard
          value={dashboard.status === 'ready' ? healthyCount : '—'}
          label="Healthy connections"
        />
        <MetricCard
          value={dashboard.status === 'ready' ? dashboard.events.length : '—'}
          label="Events needing review"
        />
      </div>
      {dashboard.status === 'loading' && (
        <div className="placeholder-note">Loading current intersection data...</div>
      )}
      {dashboard.status === 'error' && (
        <div className="placeholder-note">
          Unable to load current data.{' '}
          <button type="button" onClick={retry}>Retry</button>
        </div>
      )}
      {dashboard.status === 'ready' && dashboard.intersections.length === 0 && (
        <div className="placeholder-note">No intersections are configured yet.</div>
      )}
      {dashboard.status === 'ready' && dashboard.intersections.length > 0 && (
        <div className="feature-grid">
          <article className="feature-panel">
            <strong>Network status</strong>
            <h2>Intersections</h2>
            <IntersectionStatusList intersections={dashboard.intersections} />
          </article>
          <article className="feature-panel">
            <strong>Active conditions</strong>
            <h2>Events needing review</h2>
            <ActiveEventList events={dashboard.events} />
          </article>
        </div>
      )}
    </section>
  )
}
