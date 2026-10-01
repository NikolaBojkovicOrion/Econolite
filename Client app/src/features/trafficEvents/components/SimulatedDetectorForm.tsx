import { useState, type FormEvent } from 'react'
import type { Intersection } from '../../../types/traffic'
import { createTrafficEvent } from '../trafficEventApi'

type SimulatedDetectorFormProps = {
  intersections: Intersection[]
  onCreated: () => void
}

export function SimulatedDetectorForm({ intersections, onCreated }: SimulatedDetectorFormProps) {
  const [intersectionId, setIntersectionId] = useState(String(intersections[0]?.id ?? ''))
  const [type, setType] = useState('Congestion')
  const [severity, setSeverity] = useState('High')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    setIsSubmitting(true)
    setErrorMessage(null)
    setSuccessMessage(null)

    try {
      await createTrafficEvent(Number(intersectionId), {
        type,
        severity,
        detectedAt: new Date().toISOString(),
        sourceSystem: 'DemoDetector',
        externalEventId: crypto.randomUUID(),
      })
      setSuccessMessage('Detector event recorded.')
      onCreated()
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to record detector event.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form className="simulator-form" onSubmit={handleSubmit}>
      <label>
        Intersection
        <select
          value={intersectionId}
          onChange={(event) => setIntersectionId(event.target.value)}
          required
        >
          {intersections.map((intersection) => (
            <option key={intersection.id} value={intersection.id}>
              {intersection.name}
            </option>
          ))}
        </select>
      </label>
      <label>
        Event type
        <select value={type} onChange={(event) => setType(event.target.value)}>
          <option value="Congestion">Congestion</option>
          <option value="SlowTraffic">Slow traffic</option>
          <option value="Incident">Incident</option>
        </select>
      </label>
      <label>
        Severity
        <select value={severity} onChange={(event) => setSeverity(event.target.value)}>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
          <option value="Critical">Critical</option>
        </select>
      </label>
      <button className="simulator-submit" type="submit" disabled={isSubmitting || !intersectionId}>
        {isSubmitting ? 'Sending...' : 'Simulate detector event'}
      </button>
      {errorMessage && <p className="simulator-feedback form-error" role="alert">{errorMessage}</p>}
      {successMessage && <p className="simulator-feedback" role="status">{successMessage}</p>}
    </form>
  )
}