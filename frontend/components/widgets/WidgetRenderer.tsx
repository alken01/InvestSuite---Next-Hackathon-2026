"use client";

import { Skeleton } from "@/components/ui/skeleton";
import { WidgetCard } from "@/components/ui/widget-card";
import type { WidgetConfig } from "@/types/layout";
import { AIInsight } from "./AIInsight";
import { BuyFlow } from "./BuyFlow";
import { ComparisonTable } from "./ComparisonTable";
import { Dividends } from "./Dividends";
import { NewsDigest } from "./NewsDigest";
import { OrderStatus } from "./OrderStatus";
import { SectorHeatmap } from "./SectorHeatmap";
import { SellFlow } from "./SellFlow";
import { StockPicker } from "./StockPicker";

interface WidgetRendererProps {
  widgets: WidgetConfig[];
  accountId?: string;
  date?: string;
  onTradeComplete?: () => void;
  onQuery?: (query: string) => void;
}

function renderWidget(
  widget: WidgetConfig,
  accountId?: string,
  date?: string,
  onTradeComplete?: () => void,
  onQuery?: (query: string) => void,
) {
  switch (widget.type) {
    case "ai_insight":
      return <AIInsight data={widget.data} />;
    case "sector_heatmap":
      return <SectorHeatmap data={widget.data} />;
    case "comparison_table":
      return <ComparisonTable data={widget.data} />;
    case "buy_flow":
      return (
        <BuyFlow
          data={widget.data}
          accountId={accountId}
          date={date}
          onTradeComplete={onTradeComplete}
        />
      );
    case "sell_flow":
      return (
        <SellFlow
          data={widget.data}
          accountId={accountId}
          date={date}
          onTradeComplete={onTradeComplete}
        />
      );
    case "stock_picker":
      return <StockPicker data={widget.data} onQuery={onQuery} />;
    case "dividends":
      return <Dividends data={widget.data} />;
    case "order_status":
      return <OrderStatus data={widget.data} />;
    case "news_digest":
      return <NewsDigest data={widget.data} />;
    default:
      return null;
  }
}

export function WidgetRenderer({
  widgets,
  accountId,
  date,
  onTradeComplete,
  onQuery,
}: WidgetRendererProps) {
  if (!widgets || widgets.length === 0) return null;
  const sorted = [...widgets].sort((a, b) => a.priority - b.priority);

  return (
    <div className="widget-list">
      {sorted.map((widget, index) => (
        <div
          key={widget.id}
          style={{
            animation: `fadeInUp 0.3s cubic-bezier(0.16, 1, 0.3, 1) ${index * 50}ms both`,
            padding: "var(--space-lg)",
          }}
        >
          {renderWidget(widget, accountId, date, onTradeComplete, onQuery)}
        </div>
      ))}
    </div>
  );
}

/** Skeleton placeholder shown while widgets are loading. */
export function WidgetSkeletons() {
  return (
    <div className="widget-list">
      {/* AI insight skeleton */}
      <WidgetCard>
        <div className="flex gap-[var(--space-md)]">
          <Skeleton className="w-[3px] h-12 rounded-full bg-[var(--surface-hover)]" />
          <div className="flex-1 flex flex-col gap-[var(--space-sm)]">
            <Skeleton className="h-3.5 w-full rounded bg-[var(--surface-hover)]" />
            <Skeleton className="h-3.5 w-4/5 rounded bg-[var(--surface-hover)]" />
            <Skeleton className="h-3.5 w-3/5 rounded bg-[var(--surface-hover)]" />
          </div>
        </div>
      </WidgetCard>
      {/* Table skeleton */}
      <WidgetCard>
        <div className="flex flex-col gap-[var(--space-md)]">
          <div className="flex justify-between">
            <Skeleton className="h-3 w-16 rounded bg-[var(--surface-hover)]" />
            <Skeleton className="h-3 w-12 rounded bg-[var(--surface-hover)]" />
            <Skeleton className="h-3 w-12 rounded bg-[var(--surface-hover)]" />
          </div>
          {[1, 2, 3].map((i) => (
            <div key={i} className="flex justify-between">
              <Skeleton className="h-3.5 w-14 rounded bg-[var(--surface-hover)]" />
              <Skeleton className="h-3.5 w-16 rounded bg-[var(--surface-hover)]" />
              <Skeleton className="h-3.5 w-10 rounded bg-[var(--surface-hover)]" />
            </div>
          ))}
        </div>
      </WidgetCard>
      {/* Heatmap skeleton */}
      <WidgetCard>
        <Skeleton className="h-3.5 w-28 rounded bg-[var(--surface-hover)] mb-[var(--space-md)]" />
        <div className="grid grid-cols-4 gap-[var(--space-sm)]">
          {[1, 2, 3, 4, 5, 6, 7, 8].map((i) => (
            <Skeleton
              key={i}
              className="h-10 rounded-[var(--r-sm)] bg-[var(--surface-hover)]"
            />
          ))}
        </div>
      </WidgetCard>
    </div>
  );
}
