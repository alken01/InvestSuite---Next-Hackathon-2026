"use client";

import { AccentInsight } from "@/components/ui/accent-bar";
import { ActionButton } from "@/components/ui/action-button";
import {
  WidgetCard,
} from "@/components/ui/widget-card";
import { buyStock } from "@/lib/api";
import type { BuyFlowWidget } from "@/types/layout";
import { useEffect, useRef, useState } from "react";
import { toast } from "sonner";

interface BuyFlowProps {
  data: BuyFlowWidget["data"];
  accountId?: string;
  date?: string;
  onTradeComplete?: () => void;
}

export function BuyFlow({
  data,
  accountId,
  date,
  onTradeComplete,
}: BuyFlowProps) {
  const [shares, setShares] = useState<number>(0);
  const [buying, setBuying] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  const priceNum = data.price || 0;
  const maxCash =
    typeof data.available_cash === "number"
      ? data.available_cash
      : parseFloat(String(data.available_cash).replace(/[^0-9.]/g, "")) || 0;
  const maxShares = priceNum > 0 ? Math.floor(maxCash / priceNum) : 0;
  const totalCost = shares * priceNum;
  const deltaFormatted =
    data.delta_pct >= 0
      ? `+${data.delta_pct.toFixed(2)}%`
      : `${data.delta_pct.toFixed(2)}%`;
  const pct = maxShares > 0 ? Math.min((shares / maxShares) * 100, 100) : 0;

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

  async function handleBuy() {
    if (!accountId || shares <= 0) return;
    const tradeDate = date ?? new Date().toISOString().split("T")[0];

    setBuying(true);
    try {
      await buyStock(accountId, data.symbol, totalCost, tradeDate);
      toast.success(
        `Bought ${shares} share${shares !== 1 ? "s" : ""} of ${data.symbol} at ${data.currency}${priceNum.toFixed(2)}`,
      );
      setShares(0);
      onTradeComplete?.();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Trade failed");
    } finally {
      setBuying(false);
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
          {data.shares_owned != null && data.shares_owned > 0 && (
            <div
              style={{
                fontSize: "var(--text-xs)",
                color: "var(--green)",
                marginTop: 2,
              }}
            >
              {data.shares_owned.toFixed(data.shares_owned % 1 === 0 ? 0 : 2)} owned
            </div>
          )}
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

      {/* Shares input */}
      <div className="buy-amount-box" onClick={() => inputRef.current?.focus()}>
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
              if (e.key === "Enter" && shares > 0) handleBuy();
            }}
            className="buy-amount-input"
          />
          <span className="buy-amount-currency">shares</span>
        </div>
        {shares > 0 ? (
          <span className="buy-amount-shares">
            {data.currency}{totalCost.toFixed(2)} total
          </span>
        ) : (
          <span className="buy-amount-hint">
            up to {maxShares} shares available
          </span>
        )}
        {/* Progress bar */}
        <div className="buy-amount-bar">
          <div className="buy-amount-bar-fill" style={{ width: `${pct}%` }} />
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
          { label: "Max", fraction: 1 },
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

      {/* Buy Button */}
      <ActionButton
        variant="primary"
        fullWidth
        onClick={handleBuy}
        disabled={!accountId || shares <= 0 || buying}
      >
        {buying
          ? "Executing..."
          : shares <= 0
            ? "Enter number of shares"
            : `Buy ${shares} share${shares !== 1 ? "s" : ""} · ${data.currency}${totalCost.toFixed(2)}`}
      </ActionButton>
    </WidgetCard>
  );
}
