
import { useEffect, useState } from 'react'
import { getMenu } from '../services/api'
import PizzaBuilder from '../components/PizzaBuilder'
import type { Menu, CartPizza } from '../types'
export default function BuildPage(){
  const [menu,setMenu]=useState<Menu|null>(null)
  const [loading,setLoading]=useState(true)
  const [error,setError]=useState('')
  useEffect(()=>{ getMenu().then(setMenu).catch((e)=>setError(e.message)).finally(()=>setLoading(false)) },[])
  const add=(p:CartPizza)=>{
    const raw=localStorage.getItem('pizza-cart')
    let cart:CartPizza[]=[]
    try{ cart=raw?JSON.parse(raw):[] }catch{ cart=[] }
    if(cart.length>=10){ alert('Max 10 pizzas'); return }
    cart.push(p)
    localStorage.setItem('pizza-cart', JSON.stringify(cart))
  }
  if(loading) return <div className="grid">Loading menu...</div>
  if(error) return <div className="grid"><div className="error">Failed to load menu: {error}. Is backend running on :5000?</div></div>
  if(!menu) return <div className="grid">Empty menu</div>
  return <div className="grid"><h2>Build Your Pizza</h2><PizzaBuilder menu={menu} onAdd={add}/></div>
}
