type MetricCardProps = {
  value: number | string
  label: string
}

export function MetricCard({ value, label }: MetricCardProps) {
  return (
    <article className="metric-card">
      <strong>{value}</strong>
      <span>{label}</span>
    </article>
  )
}
