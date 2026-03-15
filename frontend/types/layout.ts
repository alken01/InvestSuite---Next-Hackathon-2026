// === Enums ===
export enum Tone {
  Reassuring = "reassuring",
  Celebratory = "celebratory",
  Neutral = "neutral",
  Focused = "focused",
  Welcoming = "welcoming",
}

export enum Density {
  Sparse = "sparse",
  Moderate = "moderate",
  Dense = "dense",
}

export enum Tier {
  Glance = "glance",
  Expanded = "expanded",
}

export enum Sentiment {
  Calm = "calm",
  Cautious = "cautious",
  Fear = "fear",
  Greed = "greed",
  Returning = "returning",
  Neutral = "neutral",
}

export enum ViewMode {
  Ambient = "ambient",
  ExpandedCard = "expanded_card",
  Research = "research",
  BuyFlow = "buy_flow",
  SellFlow = "sell_flow",
}

export enum Phase {
  Landing = "landing",
  Signup = "signup",
  Onboarding = "onboarding",
  Simulator = "simulator",
}

// === Root Payload ===
export interface LayoutPayload {
  tone: Tone;
  sentiment: Sentiment;
  view_mode: ViewMode;
  ambient?: AmbientConfig;
  center?: CenterContent;
  ai_message?: AIMessage;
  bubbles?: BubbleConfig[];
  expanded_card?: ExpandedCardConfig;
  widgets?: WidgetConfig[];
  bubble_strip?: BubbleStripItem[];
  suggestions: SuggestionChip[];
  input_placeholder?: string;
}

// === Ambient Background ===
export interface AmbientConfig {
  breathing_speed?: "slow" | "normal" | "fast";
  ring_opacity?: number;
}

// === Center Content ===
export interface CenterContent {
  value: string;
  label?: string;
  delta?: string;
  delta_color?: "positive" | "negative" | "neutral";
  delta_label?: string;
}

// === AI Message ===
export interface AIMessage {
  text: string;
  footnote?: string;
  accent?: "green" | "blue" | "amber" | "neutral";
}

// === Bubble (Floating Holding) ===
export interface BubbleConfig {
  id: string;
  ticker: string;
  delta: string;
  direction: "up" | "down" | "neutral";
  weight: number; // 0–1 fraction of total portfolio value — frontend sizes from this
  state?: "normal" | "highlighted" | "dimmed";
  has_nudge?: boolean;
  tap_query?: string;
}

// === Compact Bubble Strip ===
export interface BubbleStripItem {
  id: string;
  ticker: string;
  is_active?: boolean;
  tap_query?: string;
}

// === Expanded Card ===
export interface ExpandedCardConfig {
  name: string;
  symbol: string;
  price?: string;
  price_delta?: string;
  price_color?: string;
  subtitle?: string;
  sections: ExpandedCardSection[];
  actions?: {
    label: string;
    style: "primary" | "secondary";
    query: string;
  }[];
  footer?: string;
}

export type ExpandedCardSection =
  | { type: "insight"; text: string; accent: "green" | "blue" | "amber" }
  | { type: "education"; title: string; body: string; accent?: "green" | "blue" }
  | { type: "metrics"; items: { value: string; label: string }[] }
  | { type: "ratings"; items: { label: string; score: number; max: number }[] }
  | { type: "position"; rows: { label: string; value: string; color?: string }[] }
  | { type: "comparison"; rows: { name: string; value: string; color?: string }[] }
  | { type: "reframe"; paid: string; current: string; message: string }
  | { type: "context_card"; text: string };

// === Widget Configs ===
export type WidgetConfig =
  | SectorHeatmapWidget
  | ComparisonTableWidget
  | AIInsightWidget
  | BuyFlowWidget
  | StockPickerWidget
  | SellFlowWidget
  | DividendsWidget
  | OrderStatusWidget
  | NewsDigestWidget;

interface BaseWidget {
  id: string;
  type: string;
  tier: Tier;
  priority: number;
}

export interface SectorHeatmapWidget extends BaseWidget {
  type: "sector_heatmap";
  data: {
    title: string;
    period: string;
    items: { symbol: string; delta: number }[];
  };
}

export interface ComparisonTableWidget extends BaseWidget {
  type: "comparison_table";
  data: {
    columns: string[];
    rows: {
      symbol: string;
      is_held?: boolean;
      values: string[];
      last_col_color?: string;
    }[];
  };
}

export interface AIInsightWidget extends BaseWidget {
  type: "ai_insight";
  data: {
    text: string;
    footnote?: string;
    accent: "green" | "blue" | "amber" | "neutral";
  };
}

export interface BuyFlowWidget extends BaseWidget {
  type: "buy_flow";
  data: {
    symbol: string;
    name: string;
    price: number;
    delta_pct: number;
    suggested_amounts: number[];
    available_cash: number;
    shares_owned?: number;
    currency: string;
    ai_context: string;
    ai_context_accent: "green" | "blue" | "amber";
    order_details: { label: string; value: string }[];
  };
}

export interface StockPickerWidget extends BaseWidget {
  type: "stock_picker";
  data: {
    title: string;
    subtitle?: string;
    mode: "buy" | "sell";
    available_cash: number;
    currency: string;
    stocks: {
      symbol: string;
      name: string;
      price: number;
      delta_pct: number;
      description?: string;
      is_held?: boolean;
      shares_owned?: number;
      current_value?: number;
      return_pct?: number;
      tap_query: string;
    }[];
    ai_context?: string;
    ai_context_accent?: "green" | "blue" | "amber";
  };
}

export interface SellFlowWidget extends BaseWidget {
  type: "sell_flow";
  data: {
    symbol: string;
    name: string;
    price: number;
    delta_pct: number;
    shares_owned: number;
    current_value: number;
    average_cost: number;
    return_pct: number;
    currency: string;
    ai_context: string;
    ai_context_accent: "green" | "blue" | "amber";
    order_details: { label: string; value: string }[];
  };
}

export interface DividendsWidget extends BaseWidget {
  type: "dividends";
  data: {
    total: string;
    period: string;
    breakdown: { symbol: string; amount: string; date: string; status: "paid" | "upcoming" }[];
  };
}

export interface OrderStatusWidget extends BaseWidget {
  type: "order_status";
  data: {
    orders: {
      id: string;
      type: "buy" | "sell";
      symbol: string;
      target_price: string;
      status: "pending" | "expired" | "filled";
    }[];
  };
}

export interface NewsDigestWidget extends BaseWidget {
  type: "news_digest";
  data: {
    items: {
      headline: string;
      source: string;
      time_ago: string;
      related_symbol?: string;
      sentiment?: "positive" | "negative" | "neutral";
    }[];
  };
}

// === Key Moment (Time Travel) ===
export interface KeyMoment {
  id: number;
  date: string;
  title: string;
  description: string;
  affected_stocks: string[];
  emotion: string;
  volatility: string;
}

// === Account Creation ===
export interface CreateAccountResponse {
  id: string;
  first_name: string;
  last_name: string;
  cash_balance: number;
}

// === Trade Response ===
export interface TradeResponse {
  transaction_id: string;
  symbol: string;
  shares: number;
  price_per_share: number;
  amount: number;
  remaining_cash: number;
}

// === Suggestion Chips ===
export interface SuggestionChip {
  id: string;
  label: string;
  icon?: string;
  query: string;
  category?: "suggested" | "recent";
  is_active?: boolean;
}
