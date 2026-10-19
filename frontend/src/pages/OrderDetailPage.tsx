import { useEffect, useState } from 'react'
import { useParams, useLocation, useNavigate } from 'react-router-dom'
import { getOrder, patchStatus, deleteOrder } from '../services/api'

export default function OrderDetailPage(){
  const {id}=useParams();
  const [order,setOrder]=useState<any>(null);
  const [error,setError]=useState('');
  const loc=useLocation() as any;
  const nav=useNavigate()

  const load=async()=>{
    try{ const o=await getOrder(id!); setOrder(o)}catch(e:any){ setError(e.message)}
  }
  useEffect(()=>{ load() },[id])

  const advance=async(next:string)=>{
    try{ await patchStatus(id!,next); await load() }catch(e:any){ alert(e.response?.data?.detail||e.response?.data?.title||'409 - '+e.message) }
  }

  const del=async()=>{
    if(!confirm(`Delete Cancelled order #${id}? This cannot be undone.`)) return
    try{ await deleteOrder(id!); nav('/orders') }catch(e:any){ alert(e.response?.data?.detail||'Cannot delete - only Cancelled orders can be deleted (409)') }
  }

  if(error) return <div className="grid"><div className="error">{error}</div></div>
  if(!order) return <div className="grid"><div className="loading">Loading...</div></div>

  const allowedNext: Record<string, string[]> = {
    Pending: ['Preparing', 'Cancelled'],
    Preparing: ['Ready', 'Cancelled'],
    Ready: ['Completed'],
    Completed: [],
    Cancelled: []
  }
  const nextStatuses = allowedNext[order.status] || []

  return <div className="grid">
    {loc.state?.thankYou && <div className="card" style={{background:'#e6f9ec', borderColor:'#a7e8b5', textAlign:'center'}}>Thank you! Order #{order.id} placed.</div>}

    <div style={{display:'flex', justifyContent:'space-between', alignItems:'center', flexWrap:'wrap', gap:12}}>
      <h2>Order #{order.id} - <span className="badge">{order.status}</span></h2>
      {/* 只有 Cancelled 才顯示刪除 - 唯一一個刪除鍵 */}
      {order.status === 'Cancelled' && (
          <button onClick={del} className="btn" style={{background:'#8B0000'}}>Delete Cancelled Order</button>
      )}
    </div>

    <div className="card">
      <div><b>{order.customerName}</b> {order.phone} - {order.type}</div>
      <div style={{color:'#777', fontSize:14}}>{order.address}</div>
      <div style={{color:'#777', fontSize:13}}>Created {new Date(order.createdAt).toLocaleString()}</div>
    </div>

    <div className="card">
      <div className="label">Pizzas</div>
      {order.pizzas?.map((p:any,i:number)=>
          <div key={i} style={{padding:'8px 0', borderBottom:'1px solid #eee'}}>
            {p.size} {p.crust} {p.sauceType} {p.ingredients?.join(',')} {p.secondHalfIngredients?` / ${p.secondHalfIngredients.join(',')}`:''} - <b>${p.cost?.toFixed(2)}</b>
          </div>
      )}
      <div className="totals">
        <div><span>Subtotal</span><span>${order.subtotal?.toFixed(2)}</span></div>
        {order.discount>0 && <div><span>Discount {order.discountName}</span><span>-${order.discount?.toFixed(2)}</span></div>}
        <div><span>Delivery</span><span>${order.deliveryFee?.toFixed(2)}</span></div>
        <div><span>HST 15%</span><span>${order.tax?.toFixed(2)}</span></div>
        <div><span><b>Total</b></span><span><b>${order.total?.toFixed(2)}</b></span></div>
      </div>
    </div>

    <div className="card">
      <div className="label">Status actions (SRS 5.5)</div>
      <div style={{display:'flex',gap:8,flexWrap:'wrap', marginTop:8}}>
        {nextStatuses.length>0 ? nextStatuses.map(s=>(
            <button key={s} className={s==='Cancelled'?'btn-outline':'btn'} onClick={()=>advance(s)}>
              {s==='Cancelled'?'Cancel Order':`→ ${s}`}
            </button>
        )) : (
            <span style={{color:'#777', fontSize:14}}>
            {order.status} is final. No further actions. {order.status==='Cancelled' && 'You can delete this order using the button above.'}
          </span>
        )}
      </div>
    </div>
  </div>
}
