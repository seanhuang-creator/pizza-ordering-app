
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import HomePage from './pages/HomePage'
import BuildPage from './pages/BuildPage'
import CartPage from './pages/CartPage'
import OrdersPage from './pages/OrdersPage'
import OrderDetailPage from './pages/OrderDetailPage'
export default function App(){
  return <BrowserRouter>
    <nav style={{display:'flex',gap:12,padding:12,borderBottom:'1px solid #ddd'}}>
      <Link to="/">Sean's Pizzeria</Link>
      <Link to="/build">Build</Link>
      <Link to="/cart">Cart</Link>
      <Link to="/orders">Orders (Staff)</Link>
    </nav>
    <Routes>
      <Route path="/" element={<HomePage/>}/>
      <Route path="/build" element={<BuildPage/>}/>
      <Route path="/cart" element={<CartPage/>}/>
      <Route path="/orders" element={<OrdersPage/>}/>
      <Route path="/orders/:id" element={<OrderDetailPage/>}/>
    </Routes>
  </BrowserRouter>
}
