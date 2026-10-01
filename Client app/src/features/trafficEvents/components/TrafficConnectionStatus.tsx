import type { TrafficConnectionState } from '../useTrafficUpdates'

type TrafficConnectionStatusProps = {
  state: TrafficConnectionState
}

const stateLabels: Record<TrafficConnectionState, string> = {
  connecting: 'Connecting to live updates',
  connected: 'Live updates connected',
  reconnecting: 'Reconnecting to live updates',
  disconnected: 'Live updates disconnected',
}

export function TrafficConnectionStatus({ state }: TrafficConnectionStatusProps) {
  return (
    <p className={`connection-status connection-${state}`} role="status" aria-live="polite">
      <span className="connection-indicator" aria-hidden="true" />
      {stateLabels[state]}
    </p>
  )
}