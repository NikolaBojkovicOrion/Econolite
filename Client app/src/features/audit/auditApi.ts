import { getJson } from '../../shared/api/httpClient'

export interface AuditEntry {
  id: string
  userId: string | null
  action: string
  entityType: string
  entityId: string
  createdAt: string
}

export function getAuditEntries(signal?: AbortSignal): Promise<AuditEntry[]> {
  return getJson<AuditEntry[]>('/api/audit', signal)
}