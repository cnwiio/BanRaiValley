
---

## Variable Naming Rules for Coder Agent

### Core Philosophy
Names are the primary form of documentation. A well-named variable eliminates the need for a comment. Every name must communicate **what** the variable holds, **why** it exists, and **how** it behaves — without ever describing *how* it is stored.

---

### 1. Names Must Communicate Intent, Not Implementation

The name must reflect the **purpose** or **domain concept**, not the data type or internal structure.

✅ `userAge`, `maxRetryCount`, `invoiceTotalUsd`
❌ `intAge`, `strName`, `arrUsers` ← Hungarian notation is forbidden. Never encode the type in the name.

---

### 2. Use Standard OOP Casing Conventions

| Context | Convention | Example |
|---|---|---|
| Local variables & parameters | `camelCase` | `currentPage`, `retryCount` |
| Public properties & methods | `camelCase` (JS/TS) or `snake_case` (Python) | `getUserName()` |
| Classes & Types | `PascalCase` | `UserAccount`, `InvoiceItem` |
| Constants (compile-time) | `UPPER_SNAKE_CASE` | `MAX_RETRY_COUNT`, `DEFAULT_TIMEOUT_MS` |
| Private class fields | `_camelCase` prefix | `_cachedResult` |
| Interfaces / Abstract types | `IPascalCase` or `PascalCase` | `IRepository`, `Serializable` |

---

### 3. Boolean Variables Must Encode a True/False Question

All booleans must start with a modal verb or state verb so reading the name answers a yes/no question.

**Allowed prefixes:** `is`, `has`, `can`, `should`, `was`, `will`, `did`, `needs`

✅ `isAuthenticated`, `hasUnsavedChanges`, `canRetry`, `shouldRefetch`
❌ `authenticated`, `unsaved`, `retry`, `loading`

If negation is needed, express it through the value, not the name.
✅ `isVisible = false`
❌ `isNotVisible = true` ← Double negatives are always forbidden.

---

### 4. Names Must Be Precise — No Vague or Generic Names

Generic names carry no information and must never be used.

❌ Forbidden: `data`, `value`, `item`, `obj`, `tmp`, `temp`, `result`, `info`, `thing`, `flag`, `x`, `y` (except in mathematical/coordinate contexts)

✅ Replace with domain-specific alternatives:
- `data` → `userData`, `apiResponse`, `filteredProducts`
- `value` → `discountPercent`, `selectedOptionId`
- `tmp` → `swapBuffer`, `intermediateTotal`

---

### 5. Abbreviations Are Allowed Only When Universally Understood

Abbreviations are only acceptable when they appear in standard dictionaries, domain glossaries, or are universally recognized in your field.

✅ Allowed: `hp` (hit points), `mp` (mana points), `xp` (experience), `url`, `id`, `api`, `html`, `min`, `max`, `rgb`
❌ Forbidden: `usr`, `cnt`, `mgr`, `cfg`, `btn`, `lbl`, `calc`, `proc`

When in doubt, spell it out. Clarity beats brevity every time.

---

### 6. Collections Must Clearly Signal Plurality

Any variable holding more than one item must be obviously plural or carry a collection suffix.

**Options (choose one):**
- Plural noun: `users`, `invoiceItems`, `retryAttempts`
- Explicit suffix: `userList`, `errorArray`, `configMap`, `settingsSet`

✅ `activeUserIds`, `pendingOrderList`, `permissionMap`
❌ `user` (when it holds multiple users), `data`, `stuff`

Avoid redundant type suffixes when the plural already communicates it:
`users` is preferred over `userArray` unless the data structure distinction matters.

---

### 7. Variables With Physical or Domain Units Must Include the Unit

When a number carries a unit of measurement, the unit must appear in the name to prevent conversion errors.

**Format:** `{concept}{Unit}` using standard suffixes

| Unit | Suffix | Example |
|---|---|---|
| Milliseconds | `Ms` | `timeoutMs`, `animationDurationMs` |
| Seconds | `Sec` | `sessionDurationSec` |
| Kilometers/hour | `Kmh` | `maxSpeedKmh` |
| Meters | `M` | `altitudeM` |
| Pixels | `Px` | `borderWidthPx` |
| Bytes | `Bytes` | `fileSizeBytes` |
| Percentage | `Percent` | `discountPercent` |
| Currency (specify) | `Usd`, `Thb` | `invoiceTotalUsd` |

✅ `downloadSpeedMbps`, `requestTimeoutMs`, `priceThb`
❌ `timeout`, `speed`, `price`

---

### 8. Never Use Magic Numbers — Always Use Named Constants

Any literal number or string with a non-obvious meaning must be extracted into a named constant using `UPPER_SNAKE_CASE`.

❌ `if (retryCount > 3)` ← What is 3? Why 3?
✅ `const MAX_RETRY_COUNT = 3;` then `if (retryCount > MAX_RETRY_COUNT)`

❌ `setTimeout(callback, 5000)` ← Is 5000 ms? Is it intentional?
✅ `const SESSION_POLL_INTERVAL_MS = 5000;`

Constants must live at the module or class level, never buried inside functions.

---

### 9. Event Variables Must Follow the Handler/Emitter Pattern

Event-related names must distinguish between the **event object** itself and the **function that handles it**.

| Role | Convention | Example |
|---|---|---|
| Event handler function | `on` prefix | `onUserLogout`, `onFormSubmit`, `onPaymentSuccess` |
| Event emitter/dispatcher | `emit`/`dispatch` prefix | `emitStatusChange`, `dispatchLoginEvent` |
| Event object variable | `Event` suffix | `clickEvent`, `keyboardEvent`, `resizeEvent` |
| Event type string constant | `EVENT_` prefix (UPPER_SNAKE) | `EVENT_USER_LOGOUT`, `EVENT_PAYMENT_COMPLETE` |

✅ `onMenuToggle`, `submitEvent`, `EVENT_SESSION_EXPIRED`
❌ `handleClick`, `evt`, `e`, `myEvent`

---

### 10. Class Members Must Reflect Their Role and Visibility

In OOP classes, names must communicate ownership, lifecycle, and access level.

- **Instance state:** descriptive noun — `this.currentPage`, `this.isLoading`
- **Private fields:** `_` prefix — `this._cache`, `this._token`
- **Static/shared state:** `UPPER_SNAKE_CASE` constant or clearly prefixed — `UserService.DEFAULT_ROLE`
- **Methods:** verb + noun — `fetchUserById()`, `calculateDiscount()`, `validateEmail()`
- **Getters:** noun only (no `get` prefix in name) — `get userId()` not `getUserId()`
- **Async methods:** optionally suffix with `Async` when clarity requires — `loadConfigAsync()`

---

### 11. Naming Length Should Match Scope

The broader the scope, the more descriptive the name must be.

| Scope | Guidance | Example |
|---|---|---|
| Loop counter in 3-line loop | Short is fine | `i`, `j` |
| Block-scoped temp | Moderate | `chunk`, `page` |
| Function parameter | Descriptive | `userId`, `maxResults` |
| Module-level variable | Fully qualified | `activeDatabaseConnection` |
| Public API / exported | Most descriptive | `defaultSessionTimeoutMs` |

---

### 12. Names Must Be Stable and Refactor-Proof

Avoid names that will become misleading after common changes:
- ❌ `newUser` ← "new" relative to what? Use `pendingUser`, `unverifiedUser`
- ❌ `tempFix` ← All temp fixes become permanent. Name the intent.
- ❌ `finalResult`, `result2`, `resultV3` ← Use version control, not names, for history.
- ❌ `myFunction`, `helperUtil` ← "my" and "helper" communicate nothing.

---

These rules apply to all variable declarations, function parameters, class fields, constants, and event identifiers across all languages unless a language's official style guide directly conflicts, in which case the language guide takes precedence for casing only — all semantic rules above still apply.