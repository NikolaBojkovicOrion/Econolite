interface IntersectionFiltersProps {
  search: string
  health: string
  freshness: string
  freshnessStates: readonly string[]
  onSearchChange: (value: string) => void
  onHealthChange: (value: string) => void
  onFreshnessChange: (value: string) => void
  onClear: () => void
}

export function IntersectionFilters({
  search,
  health,
  freshness,
  freshnessStates,
  onSearchChange,
  onHealthChange,
  onFreshnessChange,
  onClear,
}: IntersectionFiltersProps) {
  return (
    <section className="intersection-toolbar" aria-label="Filter intersections">
      <label className="intersection-search">
        <span className="sr-only">Search intersection name</span>
        <input
          type="search"
          value={search}
          placeholder="Search intersection name"
          onChange={(event) => onSearchChange(event.target.value)}
        />
      </label>
      <label className="intersection-filter">
        <span className="sr-only">Filter by health</span>
        <select value={health} onChange={(event) => onHealthChange(event.target.value)}>
          <option value="All">All health statuses</option>
          <option value="Healthy">Healthy</option>
          <option value="Degraded">Degraded</option>
          <option value="Offline">Offline</option>
        </select>
      </label>
      <label className="intersection-filter">
        <span className="sr-only">Filter by detector freshness</span>
        <select value={freshness} onChange={(event) => onFreshnessChange(event.target.value)}>
          <option value="All">All freshness states</option>
          {freshnessStates.map((state) => <option key={state} value={state}>{state}</option>)}
        </select>
      </label>
      {(search || health !== 'All' || freshness !== 'All') && (
        <button className="clear-filters" type="button" onClick={onClear}>Clear filters</button>
      )}
    </section>
  )
}