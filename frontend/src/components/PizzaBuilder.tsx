import { useEffect, useState } from 'react'
import type { Menu, CartPizza } from '../types'
import { postQuote } from '../services/api'
type Props={ menu:Menu; onAdd:(p:CartPizza)=>void }

export default function PizzaBuilder({menu,onAdd}:Props){
  const [crust,setCrust]=useState('Regular')
  const [size,setSize]=useState<number|null>(null)
  const [sizeError,setSizeError]=useState('')
  const [sauce,setSauce]=useState('Regular')
  const [toppings,setToppings]=useState<string[]>([])
  const [half,setHalf]=useState(false)
  const [left,setLeft]=useState<string[]>([])
  const [right,setRight]=useState<string[]>([])
  const [livePrice,setLivePrice]=useState<number|null>(null)
  const [liveError,setLiveError]=useState('')
  const [added,setAdded]=useState(false)

  useEffect(()=>{
    if(size && crust.toLowerCase().includes('gluten') && ![10,12].includes(size)){
      setCrust('Regular')
    }
  },[size, crust])

  useEffect(()=>{
    if(size===null) { setLivePrice(null); setLiveError(''); return }
    const pizza = {
      size, crust, sauce,
      toppings: half? [] : toppings,
      isHalfAndHalf: half,
      leftToppings: left,
      rightToppings: right,
      quantity: 1
    }
    postQuote([pizza])
        .then((r:any)=>{
          const cost = r.pizzas?.[0]?.cost?? r.total?? null
          setLivePrice(cost)
          setLiveError('')
        })
        .catch((e:any)=>{
          setLiveError(e.response?.data?.detail || e.response?.data?.title || 'Quote failed')
          setLivePrice(null)
        })
  },[size,crust,sauce,toppings,left,right,half])

  const toggleWhole = (name:string)=>{
    setToppings(prev=>prev.includes(name)?prev.filter(x=>x!==name):[...prev,name])
  }
  const toggleLeft = (name:string)=>{
    setLeft(prev=>prev.includes(name)?prev.filter(x=>x!==name):[...prev,name])
  }
  const toggleRight = (name:string)=>{
    setRight(prev=>prev.includes(name)?prev.filter(x=>x!==name):[...prev,name])
  }

  const handleAdd=()=>{
    if(size===null){ setSizeError('Please choose a size'); return }
    setSizeError('')
    const pizza:CartPizza={
      id:`${Date.now()}-${Math.random()}`,
      size,
      crust,
      sauce,
      toppings: half?[]:toppings,
      isHalfAndHalf:half,
      leftToppings: half?left:undefined,
      rightToppings: half?right:undefined,
      quantity:1
    }
    onAdd(pizza)
    setAdded(true)
    setToppings([]); setLeft([]); setRight([]); setSize(null)
    setTimeout(()=>setAdded(false),3000)
  }

  return <div className="grid">
    <div className="card">
      <div className="label">Crust</div>
      <div style={{display:'flex',gap:8,flexWrap:'wrap'}}>
        {menu.crusts.map(c=>{
          const isGluten=c.name.toLowerCase().includes('gluten')
          const allowedSizesNumbers = c.allowedSizes?.map((s:any)=>parseInt(String(s),10))?? null
          const allowed =!isGluten ||!size || (allowedSizesNumbers? allowedSizesNumbers.includes(size) : [10,12].includes(size))
          return <button key={c.name} className={crust===c.name?'btn':'btn-outline'} disabled={!allowed} style={{opacity:allowed?1:0.5}} onClick={()=>setCrust(c.name)}>{c.name}{c.extraPrice?` +$${c.extraPrice}`:''}</button>
        })}
      </div>
    </div>

    <div className="card">
      <div className="label">Size * (required)</div>
      <div style={{display:'flex',gap:8,flexWrap:'wrap'}}>
        {menu.sizes.map(s=><button key={s.inches} className={size===s.inches?'btn':'btn-outline'} onClick={()=>{setSize(s.inches); setSizeError('')}}>{s.label}</button>)}
      </div>
      {sizeError && <div className="error" style={{marginTop:8}}>{sizeError}</div>}
    </div>

    <div className="card">
      <div className="label">Sauce</div>
      <div style={{display:'flex',gap:8}}>
        {menu.sauces.map(s=><button key={s.name} className={sauce===s.name?'btn':'btn-outline'} onClick={()=>setSauce(s.name)}>{s.label}</button>)}
      </div>
    </div>

    {/* FIXED: Half-and-half - checkbox not too close */}
    <div className="card">
      <label className="half-row">
        <input type="checkbox" checked={half} onChange={e=>setHalf(e.target.checked)}/>
        <span className="half-label">Half-and-half (left / right)</span>
        <span className="half-hint">Split toppings</span>
      </label>
      {half && <div style={{marginTop:12}}><div style={{fontSize:13,color:'#777'}}>Left: {left.join(', ')||'none'} | Right: {right.join(', ')||'none'}</div><div style={{display:'flex',gap:8,marginTop:8}}><button className="btn-outline" onClick={()=>{ const l=left; setLeft(right); setRight(l)}}>Swap halves</button><button className="btn-outline" onClick={()=>{setLeft([]); setRight([])}}>Clear halves</button></div></div>}
    </div>

    {!half? (
        <div className="card">
          <div className="label">Toppings</div>
          <div style={{display:'flex',gap:8,flexWrap:'wrap'}}>
            {menu.toppings.map(t=><button key={t.name} className={toppings.includes(t.name)?'btn':'btn-outline'} onClick={()=>toggleWhole(t.name)}>{t.name}{t.isPremium?'*':''} ${t.price}</button>)}
          </div>
        </div>
    ) : (
        <>
          <div className="card">
            <div className="label">Left half toppings</div>
            <div style={{display:'flex',gap:8,flexWrap:'wrap'}}>
              {menu.toppings.map(t=><button key={t.name} className={left.includes(t.name)?'btn':'btn-outline'} onClick={()=>toggleLeft(t.name)}>{t.name}{t.isPremium?'*':''} ${t.price}</button>)}
            </div>
          </div>
          <div className="card">
            <div className="label">Right half toppings</div>
            <div style={{display:'flex',gap:8,flexWrap:'wrap'}}>
              {menu.toppings.map(t=><button key={t.name+'r'} className={right.includes(t.name)?'btn':'btn-outline'} onClick={()=>toggleRight(t.name)}>{t.name}{t.isPremium?'*':''} ${t.price}</button>)}
            </div>
          </div>
        </>
    )}

    <div className="card">
      <div>Live price: {liveError?`Error: ${liveError}`: livePrice!==null?`$${livePrice.toFixed(2)}`: 'Select size'}</div>
      <button className="btn" onClick={handleAdd} style={{marginTop:12}}>Add to Order</button>
      {added && <div style={{marginTop:8}}>Added! <a href="/cart">Go to cart</a></div>}
    </div>
  </div>
}
