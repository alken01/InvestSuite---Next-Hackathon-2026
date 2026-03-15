import { LayoutPayload, KeyMoment, CreateAccountResponse, TradeResponse } from "@/types/layout";

/** Fast context fetch — computed from data, no LLM. */
export async function fetchContext(
  userId: string,
  scenario?: string,
  date?: string
): Promise<LayoutPayload> {
  const params = new URLSearchParams();
  if (scenario) params.set("scenario", scenario);
  if (date) params.set("date", date);
  const qs = params.toString();
  const res = await fetch(
    `/api/context/${encodeURIComponent(userId)}${qs ? `?${qs}` : ""}`
  );

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || `Context fetch failed: ${res.status}`);
  }

  return res.json();
}

interface LayoutRequest {
  type: "initial" | "query";
  query?: string;
  userId?: string;
  scenario?: string;
  date?: string;
}

/** LLM layout fetch — Claude-generated personalized content. */
export async function fetchLayout(params: LayoutRequest): Promise<LayoutPayload> {
  const res = await fetch("/api/layout", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      type: params.type,
      query: params.query,
      account_id: params.userId,
      scenario: params.scenario,
      date: params.date,
    }),
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || `Layout fetch failed: ${res.status}`);
  }

  return res.json();
}

/** Fetch key market moments for time-travel. */
export async function fetchKeyMoments(): Promise<KeyMoment[]> {
  const res = await fetch("/api/key-moments");
  if (!res.ok) return [];
  return res.json();
}

interface CreateAccountRequest {
  name: string;
  experienceLevel?: string;
  riskProfile?: string;
  personality?: string;
  clerkUserId?: string;
}

/** Look up an existing account by Clerk user ID. Returns null if not found. */
export async function fetchAccountByClerkId(clerkUserId: string): Promise<CreateAccountResponse | null> {
  const res = await fetch(
    `/api/accounts/by-clerk-id/${encodeURIComponent(clerkUserId)}`
  );
  if (res.status === 404) return null;
  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || `Clerk ID lookup failed: ${res.status}`);
  }
  return res.json();
}

/** Create a new simulator account with €1,000. */
export async function createAccount(params: CreateAccountRequest): Promise<CreateAccountResponse> {
  const res = await fetch("/api/accounts/create", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      name: params.name,
      experience_level: params.experienceLevel,
      risk_profile: params.riskProfile,
      personality: params.personality,
      clerk_user_id: params.clerkUserId,
    }),
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || `Account creation failed: ${res.status}`);
  }

  return res.json();
}

/** Buy a stock at a specific historical date. */
export async function buyStock(
  accountId: string,
  symbol: string,
  amount: number,
  date: string
): Promise<TradeResponse> {
  const res = await fetch(`/api/accounts/${encodeURIComponent(accountId)}/buy`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ symbol, amount, date }),
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || `Trade failed: ${res.status}`);
  }

  return res.json();
}

/** Sell shares of a stock at a specific historical date. */
export async function sellStock(
  accountId: string,
  symbol: string,
  shares: number,
  date: string
): Promise<TradeResponse> {
  const res = await fetch(`/api/accounts/${encodeURIComponent(accountId)}/sell`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ symbol, shares, date }),
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || `Trade failed: ${res.status}`);
  }

  return res.json();
}
