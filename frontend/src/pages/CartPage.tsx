
import { useEffect, useState } from 'react'
import type { CartPizza } from '../types'
import Cart from '../components/Cart'
import OrderForm from '../components/OrderForm'
import { createOrder } from '../services/api'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
export default function CartPage(){
  const [cart,setCart]=useState<CartPizza[]>(()=>{ try{ const raw=localStorage.getItem('pizza-cart'); return raw?JSON.parse(raw):[] }catch{ return [] } })
  const [totals,setTotals]=useState<any>(null)
  const [loading,setLoading]=useState(false)
  const [error,setError]=useState<string|null>(null)
  const [promo,setPromo]=useState('')
  const [promoError,setPromoError]=useState('')
  const [deliveryType,setDeliveryType]=useState<'Delivery'|'Pickup'>('Delivery')
  const [orderLoading,setOrderLoading]=useState(false)
  const [orderError,setOrderError]=useState<string|null>(null)
  const nav=useNavigate()

  useEffect(()=>{ localStorage.setItem('pizza-cart', JSON.stringify(cart)) },[cart])

  const fetchTotals=async()=>{
    if(!cart.length){ setTotals(null); return }
    setLoading(true); setError(null); setPromoError('')
    try{
      const body={ pizzas: cart.map(c=>({ size:`${c.size}"`, crust:c.crust.replace(/\s/g,''), sauceType:c.sauce, ingredients:c.isHalfAndHalf?c.leftToppings||[]:c.toppings||[], secondHalfIngredients:c.isHalfAndHalf?c.rightToppings||null:null })), promoCode:promo||null, type:deliveryType }
      const r=await axios.post('/api/quote', body)
      setTotals(r.data)
      // duplicate pizzas for quantity
      if(cart.some(c=>c.quantity>1)){
        // expand
        let expanded:any[]=[]
        cart.forEach(c=>{ for(let i=0;i<c.quantity;i++) expanded.push(c) })
        const r2=await axios.post('/api/quote',{...body, pizzas: expanded.map((c:any)=>({ size:`${c.size}"`, crust:c.crust, sauceType:c.sauce, ingredients:c.isHalfAndHalf?c.leftToppings:c.toppings, secondHalfIngredients:c.isHalfAndHalf?c.rightToppings:null }))})
        // keep original totals but use expanded? Simplified use r2
        setTotals(r2.data)
      }
    }catch(e:any){
      if(e.response?.status===400 && e.response?.data?.title?.includes('promo')){ setPromoError(e.response.data.detail||'Invalid promo code') }
      else setError(e.response?.data?.detail||e.response?.data?.title||e.message)
    }finally{ setLoading(false) }
  }
  useEffect(()=>{ fetchTotals() },[cart,deliveryType])

  const applyPromo=()=>{ fetchTotals() }

  const handleOrder=async(data:any)=>{
    setOrderLoading(true); setOrderError(null)
    try{
      const pizzas=cart.flatMap(c=> Array(c.quantity).fill(null).map(()=>({ size:`${c.size}"`, crust:c.crust.replace(/\s/g,''), sauceType:c.sauce, ingredients:c.isHalfAndHalf?c.leftToppings||[]:c.toppings||[], secondHalfIngredients:c.isHalfAndHalf?c.rightToppings||null:null })))
      const payload={ customerName:data.customerName, phone:data.customerPhone, type:data.deliveryType, address:data.customerAddress||'Pickup', promoCode:promo||null, pizzas }
      const res=await createOrder(payload)
      localStorage.removeItem('pizza-cart'); setCart([]); nav(`/orders/${res.id}`,{state:{thankYou:true}})
    }catch(e:any){ setOrderError(e.response?.data?.detail||e.response?.data?.title||'Failed') } finally{ setOrderLoading(false) }
  }

  const clear=()=>{ if(confirm('Clear cart?')){ localStorage.removeItem('pizza-cart'); setCart([]); setTotals(null) } }

  return <div className="grid">
    <h2>Cart / Checkout</h2>
    <div style={{display:'flex',gap:8}}><button className="btn-outline" onClick={clear}>Clear</button><span>Promo: <input value={promo} onChange={e=>setPromo(e.target.value.toUpperCase())} placeholder="STUDENT10"/> <button onClick={applyPromo}>Apply</button></span></div>
    {promoError && <div className="error">{promoError}</div>}
    <Cart pizzas={cart} totals={totals} onRemove={id=>setCart(prev=>prev.filter(x=>x.id!==id))} onChangeQty={(id,qty)=>{ if(qty<1)qty=1; setCart(prev=>prev.map(x=>x.id===id?{...x,quantity:qty}:x)) }} loading={loading} error={error}/>
    {cart.length>0 && <OrderForm onSubmit={handleOrder} loading={orderLoading} deliveryType={deliveryType} onDeliveryTypeChange={setDeliveryType} promoCode={promo} onPromoCodeChange={setPromo}/>}
    {orderError && <div className="error">{orderError}</div>}
  </div>
}
