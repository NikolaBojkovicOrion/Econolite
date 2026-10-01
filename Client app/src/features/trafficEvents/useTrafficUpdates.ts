import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useEffect, useRef, useState } from 'react'
import { useAuth } from '../auth'
import { getAccessToken } from '../auth/authStorage'
import { apiBaseUrl } from '../../shared/api/httpClient'
import type { AuditEntry, TrafficEvent } from '../../types/traffic'

export interface IntersectionStatusUpdate {
  intersectionId: number
  status: string
  lastDetectorUpdate: string | null
}

export type TrafficConnectionState = 'connecting' | 'connected' | 'reconnecting' | 'disconnected'

export interface TrafficUpdateHandlers {
  onTrafficEvent: (event: TrafficEvent) => void
  onIntersectionStatus: (update: IntersectionStatusUpdate) => void
  onReconnect: () => void
  onAuditEntry?: (entry: AuditEntry) => void
}

export function useTrafficUpdates(handlers: TrafficUpdateHandlers): TrafficConnectionState {
  const { user } = useAuth()
  const handlersRef = useRef(handlers)
  const [connectionState, setConnectionState] = useState<TrafficConnectionState>(
    user ? 'connecting' : 'disconnected',
  )

  handlersRef.current = handlers

  useEffect(() => {
    if (!user) {
      setConnectionState('disconnected')
      return
    }

    let disposed = false
    let retryTimer: ReturnType<typeof setTimeout> | undefined
    let retryDelayMs = 1000

    const connection = new HubConnectionBuilder()
      .withUrl(`${apiBaseUrl}/hubs/traffic`, {
        accessTokenFactory: () => getAccessToken() ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    const scheduleRetry = (): void => {
      if (disposed || retryTimer) {
        return
      }

      retryTimer = setTimeout(() => {
        retryTimer = undefined
        void startConnection()
      }, retryDelayMs)
      retryDelayMs = Math.min(retryDelayMs * 2, 30000)
    }

    const startConnection = async (): Promise<void> => {
      if (disposed) {
        return
      }

      setConnectionState('connecting')
      try {
        await connection.start()
        retryDelayMs = 1000
        setConnectionState('connected')
        if (import.meta.env.DEV) {
          console.info('SignalR connected to traffic updates.', connection.connectionId)
        }
      } catch (error) {
        if (!disposed) {
          setConnectionState('disconnected')
          if (import.meta.env.DEV) {
            console.warn('SignalR connection failed; retrying.', error)
          }
          scheduleRetry()
        }
      }
    }

    connection.on('TrafficEventCreated', (event: TrafficEvent) => {
      if (import.meta.env.DEV) console.debug('SignalR TrafficEventCreated received.', event)
      handlersRef.current.onTrafficEvent(event)
    })
    connection.on('TrafficEventUpdated', (event: TrafficEvent) => {
      if (import.meta.env.DEV) console.debug('SignalR TrafficEventUpdated received.', event)
      handlersRef.current.onTrafficEvent(event)
    })
    connection.on('IntersectionStatusUpdated', (update: IntersectionStatusUpdate) => {
      if (import.meta.env.DEV) console.debug('SignalR IntersectionStatusUpdated received.', update)
      handlersRef.current.onIntersectionStatus(update)
    })
    connection.on('AuditEntryCreated', (entry: AuditEntry) => {
      if (import.meta.env.DEV) console.debug('SignalR AuditEntryCreated received.', entry)
      handlersRef.current.onAuditEntry?.(entry)
    })
    connection.onreconnecting((error) => {
      setConnectionState('reconnecting')
      if (import.meta.env.DEV) console.warn('SignalR reconnecting.', error)
    })
    connection.onreconnected(() => {
      setConnectionState('connected')
      handlersRef.current.onReconnect()
    })
    connection.onclose(() => {
      if (!disposed) {
        setConnectionState('disconnected')
        if (import.meta.env.DEV) console.warn('SignalR connection closed.')
        scheduleRetry()
      }
    })

    void startConnection()

    return () => {
      disposed = true
      if (retryTimer) {
        clearTimeout(retryTimer)
      }
      connection.off('TrafficEventCreated')
      connection.off('TrafficEventUpdated')
      connection.off('IntersectionStatusUpdated')
      connection.off('AuditEntryCreated')
      void connection.stop()
    }
  }, [user?.id])

  return connectionState
}