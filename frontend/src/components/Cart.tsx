
export default function Cart({pizzas,totals,onRemove,onChangeQty,loading,error}:{pizzas:any[],totals:any, onRemove:(id:string)=>void, onChangeQty:(id:string,qty:number)=>void, loading?:boolean, error?:string|null}){
  if(pizzas.length===0) return <div className="card">Empty cart. <a href="/build">Build a pizza</a></div>
  return <div>
    {pizzas.map((p:any)=><div key={p.id} className="card"><div>{p.size}" {p.crust} {p.sauce} - {p.isHalfAndHalf?`Half ${p.leftToppings?.join(',')} / ${p.rightToppings?.join(',')}`:p.toppings?.join(',')||'no toppings'}</div><div><button onClick={()=>onChangeQty(p.id,p.quantity-1)}>-</button> {p.quantity} <button onClick={()=>onChangeQty(p.id,p.quantity+1)}>+</button> <button onClick={()=>onRemove(p.id)}>Remove</button></div></div>)}
    {loading && <div>Loading totals...</div>}
    {error && <div className="error">{error}</div>}
    {totals && <div className="card"><div>Subtotal ${totals.subtotal?.toFixed(2)}</div>{totals.discount>0 && <div>Discount {totals.discountName} -${totals.discount?.toFixed(2)}</div>}<div>Delivery ${totals.deliveryFee?.toFixed(2)}</div><div>HST ${totals.tax?.toFixed(2)}</div><div><b>Total ${totals.total?.toFixed(2)}</b></div></div>}
  </div>
}
