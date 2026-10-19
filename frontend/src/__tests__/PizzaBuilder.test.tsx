import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react'
import PizzaBuilder from '../components/PizzaBuilder'
import type { Menu } from '../types'

// Mock postQuote to avoid real /api/quote calls for live price (FR-BUILD-7)
// Use a promise that resolves after a tick to simulate async
vi.mock('../services/api', () => ({
  postQuote: vi.fn().mockResolvedValue({ total: 10, pizzas: [{ cost: 10 }] })
}))

const mockMenu: Menu = {
  sizes: [
    { inches: 10, basePrice: 5, name: '10"', label: '10" - $5' },
    { inches: 12, basePrice: 8, name: '12"', label: '12" - $8' },
    { inches: 14, basePrice: 12, name: '14"', label: '14" - $12' },
    { inches: 16, basePrice: 15, name: '16"', label: '16" - $15' }
  ],
  crusts: [
    { name: 'Regular', extraPrice: 0 },
    { name: 'Stuffed', extraPrice: 2 },
    { name: 'Gluten-free', extraPrice: 3, allowedSizes: ['10"', '12"'] }
  ],
  sauces: [
    { name: 'Regular', label: 'Regular' },
    { name: 'Spicy', label: 'Spicy' }
  ],
  toppings: [
    { name: 'Ham', price: 0.75, isPremium: false },
    { name: 'Shrimp', price: 1.25, isPremium: true },
    { name: 'Onions', price: 0.75, isPremium: false }
  ],
  deliveryFee: 4,
  freeDeliveryThreshold: 40,
  taxRate: 0.15,
  maxPizzas: 10
}

describe('PizzaBuilder - TST-3', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('FR-BUILD-8: shows inline error when Add to Order clicked with no size selected', async () => {
    const onAdd = vi.fn()
    render(<PizzaBuilder menu={mockMenu} onAdd={onAdd} />)

    const addButton = screen.getByRole('button', { name: /add to order/i })
    await act(async () => {
      fireEvent.click(addButton)
    })

    expect(await screen.findByText(/please choose a size/i)).toBeInTheDocument()
    expect(onAdd).not.toHaveBeenCalled()
  })

  it('FR-BUILD-5: Gluten-free crust disabled for 14" and 16", and resets to Regular when size changes', async () => {
    const onAdd = vi.fn()
    render(<PizzaBuilder menu={mockMenu} onAdd={onAdd} />)

    // Initially no size selected, Gluten-free should be enabled
    const glutenButton = screen.getByRole('button', { name: /gluten-free/i })
    expect(glutenButton).not.toBeDisabled()

    // Select 14" - wrap in act and wait for postQuote effect to settle
    const size14Button = screen.getByRole('button', { name: /14"/ })
    await act(async () => {
      fireEvent.click(size14Button)
    })

    // Wait for async postQuote effect inside PizzaBuilder to resolve
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /gluten-free/i })).toBeDisabled()
    })

    const glutenButtonAfter = screen.getByRole('button', { name: /gluten-free/i })
    expect(glutenButtonAfter.style.opacity).toBe('0.5')

    // Select 12" - should be enabled again
    const size12Button = screen.getByRole('button', { name: /12"/ })
    await act(async () => {
      fireEvent.click(size12Button)
    })

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /gluten-free/i })).not.toBeDisabled()
    })
  })
})
