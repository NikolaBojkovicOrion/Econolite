import { NavLink } from 'react-router-dom'
import { useAuth } from '../../features/auth'

export function TopBar() {
  const { user, logout } = useAuth()

  return (
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
      {user && (
        <div className="session-controls">
          <span>{user.email}</span>
          <button type="button" onClick={logout}>Sign out</button>
        </div>
      )}
    </header>
  )
}
