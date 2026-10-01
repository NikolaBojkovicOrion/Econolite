import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
} from '@tanstack/react-table'
import type { TrafficEvent } from '../../../types/traffic'
import { formatDetectorUpdate } from '../intersectionFormatting'

const eventColumns: ColumnDef<TrafficEvent>[] = [
  {
    accessorKey: 'type',
    header: 'Event',
    cell: ({ row }) => (
      <div className="intersection-name-cell">
        <strong>{row.original.type}</strong>
        <span>Event #{row.original.id.slice(0, 8)}</span>
      </div>
    ),
  },
  {
    accessorKey: 'severity',
    header: 'Severity',
    cell: ({ row }) => (
      <span className={`event-severity severity-${row.original.severity.toLowerCase()}`}>
        {row.original.severity}
      </span>
    ),
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <span className="event-status">{row.original.status}</span>,
  },
  {
    accessorKey: 'detectedAt',
    header: 'Detected',
    cell: ({ row }) => formatDetectorUpdate(row.original.detectedAt),
  },
]

interface IntersectionEventsTableProps {
  events: TrafficEvent[]
}

export function IntersectionEventsTable({ events }: IntersectionEventsTableProps) {
  const table = useReactTable({
    data: events,
    columns: eventColumns,
    getRowId: (row) => row.id,
    getCoreRowModel: getCoreRowModel(),
  })
  const rows = table.getRowModel().rows

  return (
    <section className="intersection-table-panel event-table-panel" aria-labelledby="active-events-heading">
      <div className="intersection-table-heading">
        <div>
          <span className="eyebrow">Active conditions</span>
          <h2 id="active-events-heading">Events at this intersection</h2>
        </div>
      </div>
      {rows.length === 0 ? (
        <div className="intersection-empty-state compact-empty-state">
          <h3>No active events</h3>
          <p>There are no current traffic events for this intersection.</p>
        </div>
      ) : (
        <div className="intersection-table-scroll">
          <table className="intersection-table event-table">
            <thead>
              {table.getHeaderGroups().map((headerGroup) => (
                <tr key={headerGroup.id}>
                  {headerGroup.headers.map((header) => (
                    <th key={header.id} scope="col">
                      {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
                    </th>
                  ))}
                </tr>
              ))}
            </thead>
            <tbody>
              {rows.map((row) => (
                <tr key={row.id}>
                  {row.getVisibleCells().map((cell) => (
                    <td key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}