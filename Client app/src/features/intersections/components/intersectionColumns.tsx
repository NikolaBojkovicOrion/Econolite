import { Link } from 'react-router-dom'
import type { ColumnDef } from '@tanstack/react-table'
import type { IntersectionSummary } from '../../../types/traffic'
import { formatDetectorUpdate } from '../intersectionFormatting'

export const intersectionColumns: ColumnDef<IntersectionSummary>[] = [
  {
    accessorKey: 'name',
    header: 'Intersection',
    cell: ({ row }) => (
      <div className="intersection-name-cell">
        <Link to={`/intersections/${row.original.id}`}>{row.original.name}</Link>
        <span>INT-{row.original.id}</span>
      </div>
    ),
  },
  {
    accessorKey: 'status',
    header: 'Health',
    cell: ({ row }) => (
      <span className={`status status-${row.original.status.toLowerCase()}`}>
        {row.original.status}
      </span>
    ),
  },
  {
    accessorKey: 'speedMph',
    header: 'Speed',
    cell: ({ getValue }) => {
      const speed = getValue<number | null>()
      return speed === null ? <span className="muted-value">Unavailable</span> : `${speed} mph`
    },
  },
  {
    accessorKey: 'activeEventCount',
    header: 'Active events',
  },
  {
    accessorKey: 'lastUpdate',
    header: 'Last update',
    cell: ({ row }) => formatDetectorUpdate(row.original.lastDetectorUpdate),
  },
  {
    accessorKey: 'freshness',
    header: 'Freshness',
    cell: ({ row }) => (
      <span className={`freshness-badge freshness-${row.original.freshness.toLowerCase()}`}>
        {row.original.freshness}
      </span>
    ),
  },
]