# AI-LOG.md - AI Use Log (CIS-2225 Assignment 3)

Requirement DOC-2: At least 5 entries (prompt, output, what was wrong or changed, how it was verified), including at least 2 where the AI was wrong, incomplete or insecure.

---

## Entry 1 - Frontend Test File Naming (AI was WRONG)

**Prompt:** "Generate TST-3 frontend tests to cover FR-BUILD-8 size validation and FR-BUILD-5 gluten-free disabled"

**AI Output:** AI generated files named BuildPage-Test.tsx and PizzaBuilder-Test.tsx with Vitest syntax.

**What was wrong:** Vitest configured with include pattern ["src/**/*.{test,spec}.{js,ts,jsx,tsx}"] does not match "-Test.tsx". The standard convention is ".test.tsx" or ".spec.tsx". Result: npm test showed "No test files found" or only 3 files instead of expected coverage. The tests were invisible to the runner.

**What was changed:** Renamed all files to BuildPage.test.tsx, PizzaBuilder.test.tsx, Cart.test.tsx. Updated all import paths to match.

**How verified:** Ran npm test in frontend/. Before fix: 0-3 files found. After rename: Test Files 3 passed (3) -> Tests 7 passed (7). Verified with directory listing dir src/__tests__.

---

## Entry 2 - React act() Warning in PizzaBuilder Test (AI was INCOMPLETE)

**Prompt:** "npm test shows Warning: An update to PizzaBuilder inside a test was not wrapped in act(...) - how to clear this warning?"

**AI Output:** AI first responded "tests pass, warnings are harmless, can be ignored".

**What was wrong / incomplete:** The warning was caused by async postQuote call inside useEffect for live price (FR-BUILD-7). Suggesting to ignore it is incomplete for grading - instructors expect clean test runs without React warnings. Leaving async state updates outside act() also causes flaky tests because assertions may run before state settles. This is incomplete testing practice.

**What was changed:** Wrapped all fireEvent.click calls in await act(async () => { fireEvent.click(...) }) and added await waitFor(() => expect(glutenButton).toBeDisabled()) to flush the postQuote promise. Created PizzaBuilder.test.v2.tsx with proper act handling and mock for postQuote.

**How verified:** Re-ran npm test. Before: 4x stderr | Warning: An update... plus 7 passed. After: Test Files 3 passed (3) Tests 7 passed (7) with zero stderr warnings. Duration 5.56s. Confirmed clean output in PowerShell.

---

## Entry 3 - Backend Test Discovery Location (Correct Guidance)

**Prompt:** "dotnet test --verbosity normal shows only Build succeeded, no tests executed, total: 0"

**AI Output:** AI explained that backend/ folder only contains PizzaApi.csproj with no test project, tests are in sibling backend.Tests/ folder, and dotnet test must be run at solution root where PizzaOrdering.slnx exists.

**What was wrong or changed:** No code change needed, but understanding was wrong. Running dotnet test inside backend/ yields 0 tests. Correct location is solution root. Changed execution directory to pizza-ordering-app/ root (previously pizza-ordering-system-fixed).

**How verified:** Executed at root: cd pizza-ordering-app && dotnet test. Output:
PizzaApi net8.0 succeeded -> backend\bin\...
backend.Tests net8.0 succeeded -> backend.Tests\bin\...
Test summary: total: 38, failed: 0, succeeded: 38, Build succeeded in 5.0s
Proved 38 tests discovered. Satisfies TST-4 requirement that dotnet test runs all tests.

---

## Entry 4 - Cart Total Display Test Implementation (Correct but Needed Refinement)

**Prompt:** "TST-3 requires covering cart total display FR-CART-4 and API failure FR-UI-2/3"

**AI Output:** AI generated Cart.test.tsx checking for /subtotal/i and Total: $... text, and BuildPage.test.tsx mocking fetch to reject with Failed to fetch and expecting "Unable to connect to server".

**What was changed:** Initially Cart component displayed totals only when quote available, but test regex was too strict. Adjusted test to use getByText(/\$[0-9]+\.[0-9]{2}/) and verify discount name presence for FR-CART-4. For API failure, added vi.mock for '../services/api' fallback and ensured error state renders friendly message instead of blank page per FR-UI-3.

**How verified:** npm test - Cart.test.tsx (3 tests) passed including subtotal, discount with name, delivery fee, tax, total all from server (FR-CART-4) and empty state with link to build pizza (FR-CART-8). BuildPage.test.tsx (2 tests) passed for API unreachable with clear message, no crash.

---

## Entry 5 - Gluten-Free Crust Server Validation (AI was INSECURE)

**Prompt:** "Implement FR-BUILD-5 gluten-free only for 10 and 12 inches, how to implement?"

**AI Output (First version, INSECURE):** AI initially suggested only disabling button in UI with disabled={size>12} - client-side only, no server check.

**What was wrong / insecure:** This violates NFR-1 Correctness (all prices calculated only on server) and NFR-6 Security (all input validated on server). Client-side disable can be bypassed by crafting POST /api/quote or POST /api/orders with body { size: "14\"", crust: "Gluten-free" }. An insecure implementation would accept it and produce wrong price or break business rule. This is insecure by design.

**What was changed:** Kept UI disable for usability (opacity 0.5, disabled attribute) but added server-side validation in PizzaValidator and BusinessObjects/Pizza.cs: if Crust == GlutenFree and Size not in [10,12] return 400 ProblemDetails with field error crust: "Gluten-free only for 10\" and 12\"". Also added logic in PizzaBuilder.tsx to reset crust to Regular when size changes to disallowed size, per second sentence of FR-BUILD-5. Added unit tests for this validation rule.

**How verified:** Backend unit test GlutenFree_Crust_Only_Allowed_For_10_And_12_Should_Fail - POST with 14" gluten-free returns 400 with RFC 7807 error. Frontend test verifies button disabled and resets. Both dotnet test (38 passed) and npm test (7 passed) confirm fix. Fixed warning in AppDbContext.cs by adding null checks to ValueComparer to eliminate CS8604.

---

## Summary

- Total entries: 5
- Wrong/incomplete/insecure entries: Entry 1 (wrong file naming), Entry 2 (incomplete act handling), Entry 5 (insecure client-only validation) = 3 entries satisfying DOC-2 requirement of at least 2 where AI was wrong/incomplete/insecure.
- Verification: All via dotnet test --verbosity normal (38 passed, 0 warnings after AppDbContext fix) and npm test (7 passed, no warnings) plus manual UI verification for acceptance scenarios #8, #9, #14.
