type FeaturePageProps = {
  title: string
  description: string
}

export function FeaturePage({ title, description }: FeaturePageProps) {
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
