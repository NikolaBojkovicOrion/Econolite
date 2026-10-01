import { useEffect, useState } from 'react'
import type { PaginationState } from '@tanstack/react-table'
import { ApiError } from '../../../shared/api/ApiError'
import type { IntersectionPageResponse } from '../../../types/traffic'
import { getIntersectionPage } from '../intersectionApi'
import { IntersectionDirectoryTable } from '../components/IntersectionDirectoryTable'
import { IntersectionFilters } from '../components/IntersectionFilters'

const freshnessStates = ['Fresh', 'Delayed', 'Stale'] as const
const searchDebounceMilliseconds = 300

export function IntersectionsPage() {
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [health, setHealth] = useState('All')
  const [freshness, setFreshness] = useState('All')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 5 })
  const [result, setResult] = useState<IntersectionPageResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<{ message: string; traceId?: string } | null>(null)
  const [retryCount, setRetryCount] = useState(0)
  const normalizedSearch = search.trim()

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedSearch(normalizedSearch)
      setPagination((current) => current.pageIndex === 0
        ? current
        : { ...current, pageIndex: 0 })
    }, searchDebounceMilliseconds)

    return () => window.clearTimeout(timeoutId)
  }, [normalizedSearch])

  useEffect(() => {
    if (normalizedSearch !== debouncedSearch) return

    const controller = new AbortController()
    setLoading(true)
    setError(null)

    void getIntersectionPage({
      pageNumber: pagination.pageIndex + 1,
      pageSize: pagination.pageSize,
      name: debouncedSearch || undefined,
      status: health === 'All' ? undefined : health,
      freshness: freshness === 'All' ? undefined : freshness,
    }, controller.signal).then(
      (page) => {
        setResult(page)
        setLoading(false)
      },
      (requestError: unknown) => {
        if (controller.signal.aborted || (requestError instanceof DOMException && requestError.name === 'AbortError')) {
          return
        }

        setError({
          message: requestError instanceof Error ? requestError.message : 'Unable to load intersections.',
          traceId: requestError instanceof ApiError ? requestError.traceId : undefined,
        })
        setLoading(false)
      },
    )

    return () => controller.abort()
  }, [debouncedSearch, freshness, health, normalizedSearch, pagination.pageIndex, pagination.pageSize, retryCount])

  function clearFilters(): void {
    setSearch('')
    setHealth('All')
    setFreshness('All')
    setPagination((current) => ({ ...current, pageIndex: 0 }))
  }

  return (
    <main className="page intersection-page">
      <header className="intersection-page-header">
        <div>
          <span className="eyebrow">Network status</span>
          <h1>Intersections</h1>
          <p>Monitor intersection health and the freshness of detector data.</p>
        </div>
      </header>

      <div className="intersection-metrics" aria-label="Intersection summary">
        <article className="metric-card">
          <strong>{result?.summary.totalCount ?? '—'}</strong>
          <span>Intersections monitored</span>
        </article>
        <article className="metric-card">
          <strong>{result?.summary.healthyCount ?? '—'}</strong>
          <span>Healthy intersections</span>
        </article>
        <article className="metric-card">
          <strong>{result?.summary.delayedOrStaleCount ?? '—'}</strong>
          <span>Delayed or stale detectors</span>
        </article>
      </div>

      <IntersectionFilters
        search={search}
        health={health}
        freshness={freshness}
        freshnessStates={freshnessStates}
        onSearchChange={setSearch}
        onHealthChange={(value) => {
          setHealth(value)
          setPagination((current) => ({ ...current, pageIndex: 0 }))
        }}
        onFreshnessChange={(value) => {
          setFreshness(value)
          setPagination((current) => ({ ...current, pageIndex: 0 }))
        }}
        onClear={clearFilters}
      />

      <IntersectionDirectoryTable
        result={result}
        loading={loading}
        error={error}
        pagination={pagination}
        hasActiveFilters={Boolean(normalizedSearch) || health !== 'All' || freshness !== 'All'}
        onPaginationChange={setPagination}
        onRetry={() => setRetryCount((count) => count + 1)}
        onClearFilters={clearFilters}
      />
    </main>
  )
}