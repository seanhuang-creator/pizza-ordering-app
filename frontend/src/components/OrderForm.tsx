
import { useState } from 'react'
export default function OrderForm({onSubmit,loading,deliveryType,onDeliveryTypeChange,promoCode,onPromoCodeChange}:{onSubmit:(d:any)=>void, loading:boolean, deliveryType:'Delivery'|'Pickup', onDeliveryTypeChange:(t:any)=>void, promoCode:string, onPromoCodeChange:(s:string)=>void}){
  const [name,setName]=useState(''); const [phone,setPhone]=useState(''); const [address,setAddress]=useState(''); const [nameErr,setNameErr]=useState(''); const [phoneErr,setPhoneErr]=useState(''); const [addrErr,setAddrErr]=useState('')
  const submit=()=>{
    let ok=true
    if(!name.trim()||name.length>100){ setNameErr('Name required max 100'); ok=false } else setNameErr('')
    const digits=phone.replace(/\D/g,''); if(digits.length!==10){ setPhoneErr('Phone must be 10 digits'); ok=false } else setPhoneErr('')
    if(deliveryType==='Delivery' && !address.trim()){ setAddrErr('Address required for Delivery'); ok=false } else setAddrErr('')
    if(!ok) return
    onSubmit({ customerName:name, customerPhone:phone, customerAddress:address, deliveryType, promoCode })
  }
  return <div className="card">
    <div style={{display:'flex',gap:8,marginBottom:8}}><button className={deliveryType==='Pickup'?'btn':'btn-outline'} onClick={()=>onDeliveryTypeChange('Pickup')}>Pickup</button><button className={deliveryType==='Delivery'?'btn':'btn-outline'} onClick={()=>onDeliveryTypeChange('Delivery')}>Delivery</button></div>
    <div className="label">Name *</div><input value={name} onChange={e=>setName(e.target.value)} style={{width:'100%'}}/>{nameErr && <div className="error">{nameErr}</div>}
    <div className="label" style={{marginTop:8}}>Phone * (10 digits)</div><input value={phone} onChange={e=>setPhone(e.target.value)} style={{width:'100%'}}/>{phoneErr && <div className="error">{phoneErr}</div>}
    {deliveryType==='Delivery' && <><div className="label" style={{marginTop:8}}>Address *</div><input value={address} onChange={e=>setAddress(e.target.value)} style={{width:'100%'}}/>{addrErr && <div className="error">{addrErr}</div>}</>}
    <div className="label" style={{marginTop:8}}>Promo Code</div><input value={promoCode} onChange={e=>onPromoCodeChange(e.target.value.toUpperCase())} style={{width:'100%'}} placeholder="STUDENT10"/>
    <button className="btn" style={{marginTop:12}} disabled={loading} onClick={submit}>{loading?'Placing...':'Place Order'}</button>
  </div>
}
