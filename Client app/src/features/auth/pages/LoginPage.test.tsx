import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuth } from '../useAuth'
import { LoginPage } from './LoginPage'

vi.mock('../useAuth', () => ({
  useAuth: vi.fn(),
}))

const mockedUseAuth = vi.mocked(useAuth)

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('submits credentials through the auth provider', async () => {
    const login = vi.fn().mockResolvedValue(undefined)
    mockedUseAuth.mockReturnValue({ session: null, user: null, login, logout: vi.fn() })

    render(<MemoryRouter><LoginPage /></MemoryRouter>)
    fireEvent.change(screen.getByLabelText('Email'), { target: { value: 'operator@econolite.local' } })
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'Operator123!' } })
    fireEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    await waitFor(() => expect(login).toHaveBeenCalledWith({
      email: 'operator@econolite.local',
      password: 'Operator123!',
    }))
  })

  it('shows an error when authentication fails', async () => {
    const login = vi.fn().mockRejectedValue(new Error('Unauthorized'))
    mockedUseAuth.mockReturnValue({ session: null, user: null, login, logout: vi.fn() })

    render(<MemoryRouter><LoginPage /></MemoryRouter>)
    fireEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Invalid email or password.')
  })
})
