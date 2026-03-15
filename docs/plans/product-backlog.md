# Product Backlog

Work through these items one by one, making a separate commit for each.

## Tasks

### 1. Rename BrainService
- **Status:** DONE
- **Scope:** Backend
- **Description:** Renamed `BrainService` → `AdaptiveLayoutService`. Updated interface, controller, DI registration, and codebase map.

### 2. Responsive desktop layout
- **Status:** DONE
- **Scope:** Frontend
- **Description:** The site is mobile-first but currently unusable on desktop/PC browsers. Add a centered, max-width container so it looks reasonable on wide screens. Keep mobile-first, just make it not broken on desktop.
- **Notes:** Think max-width wrapper, centered content, reasonable padding at large breakpoints.

### 3. In-memory cache for Claude API requests
- **Status:** TODO
- **Scope:** Backend
- **Description:** Add simple in-memory caching for Claude API requests. If the same request is made twice, return the cached response instead of calling the API again. No Redis or external cache — just a basic in-memory dictionary/cache with a reasonable TTL.
- **Notes:** Keep it simple. A `ConcurrentDictionary` or `IMemoryCache` is fine.

### 4. User-provided Claude API key on signup
- **Status:** TODO
- **Scope:** Full-stack
- **Description:** Allow users to provide their own Claude API key when creating an account. Store it securely. Use the user's key (if provided) instead of the app's default key when making Claude API calls for that user.
- **Notes:** Add a field to the signup flow (frontend) and persist it on the account (backend). Validate the key format.

### 5. Portfolio performance line chart
- **Status:** TODO
- **Scope:** Frontend
- **Description:** When the user taps the main account balance, expand a line chart showing portfolio value over time. Data is already available from transaction history and historical prices — just needs a visualization.
- **Notes:** Keep it simple — a single line, clear axis labels. Could use a lightweight charting lib (e.g. recharts or lightweight-charts).

### 6. Skeleton loading states
- **Status:** DONE
- **Scope:** Frontend
- **Description:** Replace spinners/blank screens with skeleton shimmer states while waiting for the AI layout response. Should match the rough shape of the widgets that will load in.
- **Notes:** Skeleton for the widget grid, bubble strip, and AI message areas.

### 7. Notification / toast system
- **Status:** DONE
- **Scope:** Frontend
- **Description:** Add a toast notification system for trade confirmations, errors, and status updates. Should be non-intrusive and auto-dismiss.
- **Notes:** Use Radix UI Toast or sonner. Keep consistent with the glass/ambient design language.

### 8. Historical stock performance query
- **Status:** TODO
- **Scope:** Full-stack
- **Description:** Let users ask "How has ASML done in the past 6 months?" and get a response with real historical price data. We already fetch Yahoo Finance data for time-travel — reuse that to answer performance questions over a time range.
- **Notes:** Backend needs an endpoint or prompt enrichment that pulls historical prices for a symbol + period. Frontend could show a mini chart or text summary. Keep it informational, no advice.

### 9. Anonymized "learn from others" view
- **Status:** IDEA (lowest priority)
- **Scope:** Full-stack
- **Description:** Let investors see anonymized strategies or portfolio compositions from other profiles. e.g. "Investors with similar risk profiles hold X% in ETFs." Not advice — just perspective.
- **Notes:** Think about this more before building. Could be static/mock data initially. Must not violate the "no advice" rule — frame as information only.

### 10. Input bar cleanup
- **Status:** DONE
- **Scope:** Frontend
- **Description:** Two fixes for the input bar: (1) Remove the ⌘ command icon — it's confusing and unnecessary. (2) Make the input support multiple lines (textarea instead of single-line input) so users can type longer queries.
- **Notes:** Keep auto-grow behavior reasonable — maybe 3-4 lines max before scrolling. Submit on Enter, Shift+Enter for newline.

### 11. Bubble collision avoidance
- **Status:** DONE
- **Scope:** Frontend
- **Description:** Floating portfolio bubbles currently overlap with each other and with the center content / AI message / input bar. Fix positioning so bubbles keep minimum distance from each other and stay clear of other UI components.
- **Notes:** Could use simple collision detection on initial placement, or constrain positions to safe zones. Also check that bubbles don't drift into other components during float animation.

### 12. Personalized greeting on app open
- **Status:** DONE
- **Scope:** Frontend
- **Description:** Show "Hi, {first name}" when the user opens the app. For demo accounts (Sophie, Marc) use their name. For simulator accounts, use the name from signup. Display it in the center zone or as an AI message on initial load.
- **Notes:** Keep it simple and warm. Don't overdo it — just a greeting, not a full welcome screen.

### 13. Code quality review pass
- **Status:** TODO (do last)
- **Scope:** Full-stack
- **Description:** Full review of frontend and backend code quality. Use `/review-conventions`, `/review-dotnet`, `/review-frontend`, `/review-tests` to check compliance and fix violations.
- **Notes:** This is a cleanup pass — do it after the feature work above is done.
