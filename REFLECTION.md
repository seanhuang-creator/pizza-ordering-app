# Reflection - CIS-2225 Assignment 3

Word count: 354 words. Covers DOC-3 requirements: what AI did well, where it failed, a design decision made against its suggestion, and why a static in-memory list is a poor fit for a web API.

## What AI did well

AI accelerated scaffolding for the pizza ordering system, especially boilerplate mapped to the SRS. Given FR-MENU-1 and API Requirements in Section 7, it generated correct DTOs, Swagger/OpenAPI setup, CORS configuration, and the BusinessObjects folder structure with Pizza.CalculateCost() and overridden ToString() as required in Section 6. For business rules, it correctly implemented BR-1 (size base + crust + toppings) and BR-2 (half-and-half cost divided by 2) with rounding half away from zero, and BR-7 total formula. It also produced the initial 33 backend unit tests covering each size base price, crust surcharge, premium toppings, half-and-half, and both discount rules as separate testable rule classes implementing IDiscountRule per BR-6. This separation makes it easy to add new discounts without editing pricing logic. The AI-generated frontend components from menu API response without hard-coding (FR-MENU-3) were also correct.

## Where AI failed

AI failed on tooling conventions and security boundaries. First, it named frontend tests BuildPage-Test.tsx, but Vitest only matches *.test.tsx pattern, so npm test found no files. Second, it ignored React act() requirement for async postQuote live price effect (FR-BUILD-7), producing "An update to PizzaBuilder was not wrapped in act(...)" warnings. Suggesting to ignore warnings is incomplete for TST-4. Third and most critical, it suggested only disabling Gluten-free crust in UI for 14" and 16". This violates NFR-1 (prices only on server) and NFR-6 (server validation) because a client can bypass UI and POST a 14" Gluten-free pizza directly. The server must reject it with 400 ProblemDetails.

## Design decision made against AI suggestion

AI suggested keeping cart total calculation in frontend for speed and storing orders in a static List<Order> in memory for simplicity. I rejected both. Per NFR-1 and FR-ORD-2, I forced all pricing to be recalculated on server in QuoteService and OrderService, ignoring any client price (acceptance #7). For orders, I used SQLite via EF Core with migrations running on startup, not a static list.

A static in-memory list is a poor fit for a web API because: (1) ASP.NET Core controllers are instantiated per request, but static state is shared across all threads, causing race conditions without explicit locking. (2) Data is lost on API restart, failing acceptance #11 where an order must still be listed after restart. (3) It cannot support filtering, paging (FR-MGT-2, API-4), or transactions. (4) It breaks when scaling to multiple instances or when the app pool recycles. (5) Deleting an order would not cascade to pizzas reliably. EF Core with SQLite solves persistence, concurrency via DbContext per request, and proper paging with total count, and meets Data Requirements that schema be created by migrations.

This experience taught me to use AI as a generator but verify every line against SRS IDs, business rules, and security requirements myself.
