
import axios from 'axios'
const API = axios.create({ baseURL: '' })
export async function getMenu(){ const r=await API.get('/api/menu'); const m=r.data; return { sizes:m.sizes.map((s:any)=>({inches:parseInt(s.name), basePrice:s.basePrice, name:s.name, label:`${s.name}\" - $${s.basePrice}`})), crusts:m.crusts.map((c:any)=>({name:c.name, extraPrice:c.extraPrice, allowedSizes:c.allowedSizes?.map((x:string)=>parseInt(x))})), sauces:m.sauces.map((s:any)=>({name:s.name,label:s.name})), toppings:m.toppings.map((t:any)=>({name:t.name,price:t.price,isPremium:t.isPremium})), deliveryFee:m.deliveryFee, freeDeliveryThreshold:m.freeDeliveryThreshold, taxRate:m.taxRate, maxPizzas:m.maxPizzasPerOrder } }
export async function postQuote(pizzas:any[], promoCode?:string, type:string='Pickup'){ const body={ pizzas: pizzas.map(p=>({ size:`${p.size}`, crust:p.crust.replace(/\s/g,''), sauceType:p.sauce, ingredients:p.isHalfAndHalf?p.leftToppings||[]:p.toppings||[], secondHalfIngredients:p.isHalfAndHalf?p.rightToppings||null:null })), promoCode:promoCode||null, type }; const r=await API.post('/api/quote', body); return r.data }
export async function createOrder(payload:any){ const r=await API.post('/api/orders', payload); return r.data }
export async function getOrders(params:any={}){ const r=await API.get('/api/orders',{params}); return r.data }
export async function getOrder(id:string){ const r=await API.get(`/api/orders/${id}`); return r.data }
export async function patchStatus(id:string,status:string){ const r=await API.patch(`/api/orders/${id}/status`,{status}); return r.data }
export async function deleteOrder(id:string){ const r=await API.delete(`/api/orders/${id}`); return r.data }
