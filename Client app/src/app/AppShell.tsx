import { Route, Routes } from 'react-router-dom'
import { TopBar } from './components/TopBar'
import { DashboardPage } from '../features/dashboard/pages/DashboardPage'
import { RequireAuth } from '../features/auth/components/RequireAuth'
import { LoginPage } from '../features/auth/pages/LoginPage'
import { IntersectionsPage } from '../features/intersections/pages/IntersectionsPage'
import { IntersectionDetailPage } from '../features/intersections/pages/IntersectionDetailPage'
import { FeaturePage } from '../shared/components/FeaturePage'

export function AppShell() {
  return (
    <div className="app-shell">
      <TopBar />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<RequireAuth />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/intersections" element={<IntersectionsPage />} />
          <Route path="/intersections/:id" element={<IntersectionDetailPage />} />
          <Route
            path="/events"
            element={<FeaturePage title="Traffic events" description="Review congestion, detector signals, and incidents that need attention." />}
          />
          <Route
            path="/audit"
            element={<FeaturePage title="Audit history" description="Trace important operator and system actions through an accountable record." />}
          />
        </Route>
        <Route
          path="*"
          element={<LoginPage />}
        />
      </Routes>
    </div>
  )
}
