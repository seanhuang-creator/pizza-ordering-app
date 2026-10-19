
import { Link } from 'react-router-dom'
export default function HomePage(){ return <div className="grid"><div className="card" style={{textAlign:'center'}}><img src="https://images.unsplash.com/photo-1513104890138-7c749659a591?w=600" alt="pizza" style={{maxWidth:'100%',borderRadius:8}}/><h1>Sean's Pizzeria</h1><p>Build your pizza, fresh and hot</p><Link to="/build" className="btn">Start Order</Link></div></div> }
