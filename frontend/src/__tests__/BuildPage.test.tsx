import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import BuildPage from '../pages/BuildPage'

// Mock the api module for menu loading
vi.mock('../services/api', () => ({
  getMenu: vi.fn()
}))

import { getMenu } from '../services/api'

describe('BuildPage - TST-3 API failure', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('FR-UI-2 / FR-UI-3: shows friendly error when menu API fails, not blank page', async () => {
    const mockedGetMenu = vi.mocked(getMenu)
    mockedGetMenu.mockRejectedValue(new Error('Network Error'))

    render(<BuildPage />)

    // Should show loading first
    expect(screen.getByText(/loading menu/i)).toBeInTheDocument()

    // After failure, should show error with clear message, not crash
    const errorMessage = await screen.findByText(/failed to load menu/i)
    expect(errorMessage).toBeInTheDocument()
    expect(errorMessage.textContent).toMatch(/is backend running/i)
  })

  it('FR-UI-2: shows empty state when menu is null', async () => {
    const mockedGetMenu = vi.mocked(getMenu)
    mockedGetMenu.mockResolvedValue(null as any)

    render(<BuildPage />)

    const emptyState = await screen.findByText(/empty menu/i)
    expect(emptyState).toBeInTheDocument()
  })
})
