import { useEffect, useState } from 'react'
import { getOrders } from '../services/api'
import { Link } from 'react-router-dom'

export default function OrdersPage(){
  const [orders,setOrders]=useState<any[]>([])
  const [total,setTotal]=useState(0)
  const [status,setStatus]=useState('')
  const [from,setFrom]=useState('')
  const [to,setTo]=useState('')
  const [page,setPage]=useState(1)

  const load=async()=>{
    const r=await getOrders({status:status||undefined, from:from||undefined, to:to||undefined, page, pageSize:20});
    setOrders(r.items||r.Items||[]);
    setTotal(r.totalCount||r.TotalCount||0)
  }
  useEffect(()=>{ load() },[status,from,to,page])

  return <div className="grid"><h2>Orders - Staff</h2>
    <div className="filter-row">
      <div className="filter-group">
        <label>Status</label>
        <select value={status} onChange={e=>setStatus(e.target.value)}>
          <option value="">All status</option>
          <option>Pending</option>
          <option>Preparing</option>
          <option>Ready</option>
          <option>Completed</option>
          <option>Cancelled</option>
        </select>
      </div>
      <div className="filter-group">
        <label>From</label>
        <input type="date" value={from} onChange={e=>setFrom(e.target.value)}/>
      </div>
      <div className="filter-group">
        <label>To</label>
        <input type="date" value={to} onChange={e=>setTo(e.target.value)}/>
      </div>
    </div>

    {/* Orders list - centered */}
    <div className="grid" style={{justifyItems:'center'}}>
      {orders.map((o:any)=>
          <div key={o.id} className="card" style={{width:'100%', maxWidth:'640px', textAlign:'center'}}>
            <Link to={`/orders/${o.id}`}>
              #{o.id} {o.customerName} {o.type} {new Date(o.createdAt).toLocaleString()} {o.pizzas?.length||0} pizzas ${o.total?.toFixed(2)} {o.status}
            </Link>
          </div>
      )}
      {orders.length===0 && <div className="empty" style={{maxWidth:'640px', width:'100%'}}>No orders found</div>}
    </div>

    {/* Pagination - CENTERED */}
    <div className="pagination">
      <button disabled={page<=1} onClick={()=>setPage(p=>p-1)}>Prev</button>
      <span>Page {page} Total {total}</span>
      <button onClick={()=>setPage(p=>p+1)}>Next</button>
    </div>
  </div>
}
