import { getJson } from '../../shared/api/httpClient'
import type { AuditEntry } from '../../types/traffic'

export type { AuditEntry } from '../../types/traffic'

export function getAuditEntries(signal?: AbortSignal): Promise<AuditEntry[]> {
  return getJson<AuditEntry[]>('/api/audit', signal)
}