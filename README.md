# Sean's Pizzeria - Pizza Ordering System

CIS-2225 Assignment 3 - React (Vite) + ASP.NET Core Web API + SQLite

## Architecture

- **Frontend**: React 18 + TypeScript + Vite (components: PizzaBuilder, Cart, BuildPage). All size, crust, sauce, and topping controls are built from GET /api/menu response - no hard-coded menu items or prices (FR-MENU-3). Cart persists in localStorage (FR-CART-7).
- **Backend**: C# ASP.NET Core 8 Web API with EF Core + SQLite. Domain logic in BusinessObjects/ folder: Pizza with CalculateCost() and ToString(), Order, and discount rule classes (TwoForTuesdayRule, Student10Rule) implementing IDiscountRule for BR-6 extensibility.
- **Pricing**: Server-side only (NFR-1). POST /api/quote and POST /api/orders recalculate all costs and ignore client prices. Tax = 15% * (subtotal - discount), delivery fee $4 free when subtotal before discount > $40, BR-7 rounding half away from zero.
- **Database**: EF Core migrations run on startup in Program.cs. Orders and pizzas stored in SQLite pizza.db, survive API restart. Deleting an order cascades to pizzas.
- **API**: Swagger enabled at /swagger. DTOs for all requests/responses (API-1), RFC 7807 ProblemDetails with field-level 400 errors (API-2), enums as strings (API-3), CORS allows frontend origin.

## How to Install

Prerequisites: .NET 8 SDK, Node.js 18+, npm

```bash
git clone https://github.com/seanhuang-creator/pizza-ordering-app.git
cd pizza-ordering-app

# Backend
cd backend
dotnet restore

# Frontend
cd ../frontend
npm install
```

No secrets in repository (NFR-6).

## How to Run

Backend - one command, at solution root or backend folder:

```bash
# Option A - at solution root (recommended, runs migrations automatically)
dotnet run --project backend/PizzaApi.csproj
# API: https://localhost:7056 or http://localhost:5000
# Swagger: https://localhost:7056/swagger

# Option B - inside backend folder
cd backend
dotnet run
```

Frontend - one command, in frontend folder:

```bash
cd frontend
npm run dev
# App: http://localhost:5173
# Configure VITE_API_URL in .env if backend port differs
```

Backend and frontend each start with one documented command (Technical Constraints - Run).

## How to Test

Backend - 38 tests (TST-1 + TST-2) at solution root:

```bash
# At pizza-ordering-app/ root (contains PizzaOrdering.slnx)
dotnet test

# Verbose to see each test name
dotnet test --verbosity normal

# Expected output:
# PizzaApi net8.0 succeeded -> backend/bin/Debug/net8.0/PizzaApi.dll
# backend.Tests net8.0 succeeded -> backend.Tests/bin/Debug/net8.0/backend.Tests.dll
# Test summary: total: 38, failed: 0, succeeded: 38

# Coverage:
# - each size (10/12/14/16), crust surcharges, premium toppings, half-and-half (BR-2)
# - TwoForTuesday, STUDENT10, discount precedence (BR-3)
# - tax rounding (BR-7), delivery threshold (> $40 free)
# - status transitions (Pending->Preparing->Ready->Completed, Cancelled delete rule)
```

Frontend - 7 tests (TST-3) in frontend folder:

```bash
cd frontend
npm test
# or
npm test -- --run

# Expected:
# src/__tests__/Cart.test.tsx (3)
# src/__tests__/BuildPage.test.tsx (2)
# src/__tests__/PizzaBuilder.test.tsx (2)
# Test Files 3 passed, Tests 7 passed
# Covers: FR-BUILD-8 size validation, FR-BUILD-5 gluten-free disabled,
# FR-CART-4 total display, FR-CART-8 empty cart, FR-UI-2/3 API failure
```

All tests pass using dotnet test and npm test (TST-4).

## Acceptance Scenarios Quick Verification

1. Price 12" Regular Ham pickup Monday: subtotal $8.75, tax $1.31, total $10.06 via POST /api/quote
2. Price 14" half Ham+Onions half Shrimp: pizza cost $13.38
3. Tuesday discount: Two-for-Tuesday $2.50
4. STUDENT10: $1.20 discount, total $12.42
5. Delivery fee $4, #6 free over $40
7. Client price ignored - server recalculates
8. No size selected: inline error, cart unchanged
9. 14" pizza: gluten-free disabled
10. Delivery no address: 400 with field error
11. Order survives API restart (SQLite)
12. Preparing -> Completed: 409
13. Delete non-Cancelled: 409, after Cancel -> 204
14. Stop API: friendly error, no crash
15. Refresh: cart still there (localStorage)

## Ports and Configuration

- Backend default: http://localhost:5000 (see backend/Properties/launchSettings.json)
- Frontend expects API at http://localhost:5000 - change via frontend/.env: VITE_API_URL=http://localhost:5000
- CORS: Program.cs allows http://localhost:5173

