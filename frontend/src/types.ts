
export type Menu = { sizes: { inches:number; basePrice:number; name:string; label:string }[]; crusts:{name:string; extraPrice:number; allowedSizes?:string[]}[]; sauces:{name:string; label:string}[]; toppings:{name:string; price:number; isPremium:boolean}[]; deliveryFee:number; freeDeliveryThreshold:number; taxRate:number; maxPizzas:number }
export type CartPizza = { id:string; size:number; crust:string; sauce:string; toppings:string[]; isHalfAndHalf:boolean; leftToppings?:string[]; rightToppings?:string[]; quantity:number }
export type CartTotals = { subtotal:number; deliveryFee:number; tax:number; discount:number; discountName?:string; total:number; pizzas?: any[] }
