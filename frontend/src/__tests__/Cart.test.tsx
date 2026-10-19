import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import Cart from '../components/Cart'

describe('Cart - TST-3', () => {
  it('FR-CART-8: shows empty state with link to build when cart is empty', () => {
    render(<Cart pizzas={[]} totals={null} onRemove={vi.fn()} onChangeQty={vi.fn()} />)

    expect(screen.getByText(/empty cart/i)).toBeInTheDocument()
    const link = screen.getByRole('link', { name: /build a pizza/i })
    expect(link).toBeInTheDocument()
    expect(link.getAttribute('href')).toBe('/build')
  })

  it('FR-CART-4: shows subtotal, discount, delivery fee, tax and total from server', () => {
    const pizzas = [
      {
        id: '1',
        size: 12,
        crust: 'Regular',
        sauce: 'Regular',
        toppings: ['Ham'],
        isHalfAndHalf: false,
        quantity: 1
      }
    ]
    const totals = {
      subtotal: 8.75,
      discount: 0,
      discountName: null,
      deliveryFee: 0,
      tax: 1.31,
      total: 10.06
    }

    render(<Cart pizzas={pizzas as any} totals={totals} onRemove={vi.fn()} onChangeQty={vi.fn()} />)

    // Check server-calculated values are displayed
    expect(screen.getByText(/subtotal \$8\.75/i)).toBeInTheDocument()
    expect(screen.getByText(/total \$10\.06/i)).toBeInTheDocument()
    expect(screen.getByText(/hst \$1\.31/i)).toBeInTheDocument()
    expect(screen.getByText(/delivery \$0\.00/i)).toBeInTheDocument()
  })

  it('FR-CART-4: shows discount with name when discount > 0', () => {
    const pizzas = [
      {
        id: '1',
        size: 14,
        crust: 'Regular',
        sauce: 'Regular',
        toppings: ['Ham'],
        isHalfAndHalf: false,
        quantity: 1
      }
    ]
    const totals = {
      subtotal: 12,
      discount: 1.2,
      discountName: 'STUDENT10',
      deliveryFee: 0,
      tax: 1.62,
      total: 12.42
    }

    render(<Cart pizzas={pizzas as any} totals={totals} onRemove={vi.fn()} onChangeQty={vi.fn()} />)

    expect(screen.getByText(/discount STUDENT10/i)).toBeInTheDocument()
    expect(screen.getByText(/\$1\.20/)).toBeInTheDocument()
  })
})
