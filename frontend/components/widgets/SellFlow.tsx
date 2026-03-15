"use client";

import { useEffect, useRef, useState } from "react";
import { toast } from "sonner";
import type { SellFlowWidget } from "@/types/layout";
import { WidgetCard } from "@/components/ui/widget-card";
import { AccentInsight } from "@/components/ui/accent-bar";
import { ActionButton } from "@/components/ui/action-button";
import { sellStock } from "@/lib/api";

interface SellFlowProps {
  data: SellFlowWidget["data"];
  accountId?: string;
  date?: string;
  onTradeComplete?: () => void;
}

export function SellFlow({ data, accountId, date, onTradeComplete }: SellFlowProps) {
  const [shares, setShares] = useState<number>(0);
  const [selling, setSelling] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  const priceNum = data.price || 0;
  const maxShares = data.shares_owned || 0;
  const totalValue = shares * priceNum;
  const costBasis = shares * (data.average_cost || 0);
  const gain = totalValue - costBasis;
  const gainPct = costBasis > 0 ? (gain / costBasis) * 100 : 0;
  const pct = maxShares > 0 ? Math.min((shares / maxShares) * 100, 100) : 0;
  const deltaFormatted =
    data.delta_pct >= 0
      ? `+${data.delta_pct.toFixed(2)}%`
      : `${data.delta_pct.toFixed(2)}%`;

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  function updateShares(val: number) {
    if (Number.isNaN(val) || val < 0) {
      setShares(0);
      return;
    }
    setShares(Math.min(Math.floor(val), maxShares));
  }

  async function handleSell() {
    if (!accountId || shares <= 0) return;
    const tradeDate = date ?? new Date().toISOString().split("T")[0];

    setSelling(true);
    try {
      const trade = await sellStock(accountId, data.symbol, shares, tradeDate);
      toast.success(
        `Sold ${trade.shares.toFixed(4)} shares of ${data.symbol} at ${data.currency}${trade.price_per_share.toFixed(2)}`,
      );
      setShares(0);
      onTradeComplete?.();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Trade failed");
    } finally {
      setSelling(false);
    }
  }

  return (
    <WidgetCard>
      {/* Header */}
      <div className="flex justify-between items-start mb-[var(--space-xl)]">
        <div>
          <div
            style={{
              fontSize: "var(--body-size)",
              fontWeight: 500,
              color: "var(--text-primary)",
            }}
          >
            {data.symbol}
          </div>
          <div
            style={{
              fontSize: "var(--text-small)",
              color: "var(--text-dim)",
              marginTop: 2,
            }}
          >
            {data.name}
          </div>
        </div>
        <div className="text-right">
          <div
            style={{
              fontSize: "var(--body-size)",
              fontWeight: 500,
              color: "var(--text-primary)",
            }}
          >
            {data.currency}{priceNum.toFixed(2)}
          </div>
          <div
            style={{
              fontSize: "var(--text-small)",
              color: data.delta_pct >= 0 ? "var(--green)" : "var(--red)",
              marginTop: 2,
            }}
          >
            {deltaFormatted}
          </div>
        </div>
      </div>

      {/* Position Summary */}
      <div className="grid grid-cols-3 gap-[var(--space-sm)] mb-[var(--space-lg)] p-[var(--space-md)] rounded-[var(--radius-md)] bg-[var(--surface)]">
        <div className="text-center">
          <div className="text-[var(--text-dim)] text-[11px]">Shares</div>
          <div className="text-[var(--text-primary)] text-sm font-medium">{maxShares}</div>
        </div>
        <div className="text-center">
          <div className="text-[var(--text-dim)] text-[11px]">Value</div>
          <div className="text-[var(--text-primary)] text-sm font-medium">{data.currency}{data.current_value.toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 0 })}</div>
        </div>
        <div className="text-center">
          <div className="text-[var(--text-dim)] text-[11px]">Return</div>
          <div className="text-sm font-medium" style={{ color: data.return_pct >= 0 ? "var(--green)" : "var(--red)" }}>{data.return_pct >= 0 ? "+" : ""}{data.return_pct.toFixed(1)}%</div>
        </div>
      </div>

      {/* Shares input */}
      <div className="buy-amount-box " onClick={() => inputRef.current?.focus()}>
        <div className="flex items-baseline gap-1">
          <input
            ref={inputRef}
            type="number"
            inputMode="numeric"
            min={0}
            max={maxShares}
            step={1}
            value={shares || ""}
            placeholder="0"
            onChange={(e) => updateShares(parseFloat(e.target.value))}
            onKeyDown={(e) => {
              if (e.key === "Enter" && shares > 0) handleSell();
            }}
            className="buy-amount-input"
          />
          <span className="buy-amount-currency">shares</span>
        </div>
        {shares > 0 ? (
          <span className="buy-amount-shares">
            {data.currency}{totalValue.toFixed(2)} · {gain >= 0 ? "+" : ""}{data.currency}{gain.toFixed(2)} ({gainPct >= 0 ? "+" : ""}{gainPct.toFixed(1)}%)
          </span>
        ) : (
          <span className="buy-amount-hint">
            {maxShares} share{maxShares !== 1 ? "s" : ""} available to sell
          </span>
        )}
        {/* Progress bar */}
        <div className="buy-amount-bar">
          <div className="buy-amount-bar-fill-fill" style={{ width: `${pct}%` }} />
        </div>
      </div>

      {/* Slider */}
      <input
        type="range"
        min={0}
        max={maxShares}
        step={1}
        value={shares || 0}
        onChange={(e) => updateShares(parseFloat(e.target.value))}
        className="buy-slider"
      />

      {/* Quick presets */}
      <div className="flex gap-[var(--space-sm)] mb-[var(--space-lg)]">
        {[
          { label: "25%", fraction: 0.25 },
          { label: "50%", fraction: 0.5 },
          { label: "All", fraction: 1 },
        ].map(({ label, fraction }) => {
          const presetVal = Math.floor(maxShares * fraction);
          return (
            <button
              key={label}
              onClick={() => setShares(presetVal)}
              className={`cursor-pointer quick-amount-btn flex-1 ${shares === presetVal ? "isActive" : ""}`}
            >
              {label}
            </button>
          );
        })}
      </div>

      {/* AI Context */}
      {data.ai_context && (
        <div className="mb-[var(--space-md)]">
          <AccentInsight
            text={data.ai_context}
            accent={data.ai_context_accent}
          />
        </div>
      )}

      {/* Sell Button */}
      <ActionButton
        variant="primary"
        fullWidth
        onClick={handleSell}
        disabled={!accountId || shares <= 0 || selling}
      >
        {selling
          ? "Executing..."
          : shares <= 0
            ? "Enter number of shares"
            : `Sell ${shares} share${shares !== 1 ? "s" : ""} · ${data.currency}${totalValue.toFixed(2)}`}
      </ActionButton>
    </WidgetCard>
  );
}
