import { Route, Routes } from 'react-router-dom'
import { TopBar } from './components/TopBar'
import { DashboardPage } from '../features/dashboard/pages/DashboardPage'
import { FeaturePage } from '../shared/components/FeaturePage'

export function AppShell() {
  return (
    <div className="app-shell">
      <TopBar />
      <Routes>
        <Route path="/" element={<DashboardPage />} />
        <Route
          path="/intersections"
          element={<FeaturePage title="Intersections" description="Monitor the health and freshness of every roadway connection." />}
        />
        <Route
          path="/events"
          element={<FeaturePage title="Traffic events" description="Review congestion, detector signals, and incidents that need attention." />}
        />
        <Route
          path="/audit"
          element={<FeaturePage title="Audit history" description="Trace important operator and system actions through an accountable record." />}
        />
        <Route
          path="*"
          element={<FeaturePage title="Not found" description="The requested workspace view does not exist." />}
        />
      </Routes>
    </div>
  )
}
