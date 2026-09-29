import { NavLink } from 'react-router-dom'

export function TopBar() {
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
    </header>
  )
}
