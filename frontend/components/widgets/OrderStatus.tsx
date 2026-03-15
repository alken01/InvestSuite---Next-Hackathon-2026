"use client";

import type { OrderStatusWidget } from "@/types/layout";
import { WidgetCard } from "@/components/ui/widget-card";
import { StatusBadge } from "@/components/ui/status-badge";

const statusConfig: Record<string, { color: string; label: string }> = {
  pending: { color: "var(--blue)", label: "Pending" },
  expired: { color: "var(--amber)", label: "Expired" },
  filled: { color: "var(--green)", label: "Filled" },
};

interface OrderStatusProps {
  data: OrderStatusWidget["data"];
}

export function OrderStatus({ data }: OrderStatusProps) {
  const orders = data.orders ?? [];

  return (
    <WidgetCard>
      <div
        style={{
          fontSize: "var(--text-xs)",
          color: "var(--text-dim)",
          textTransform: "uppercase",
          letterSpacing: "0.5px",
          marginBottom: "var(--space-lg)",
        }}
      >
        Orders
      </div>

      <div className="flex flex-col">
        {orders.map((order, i) => {
          const config = statusConfig[order.status] ?? { color: "var(--text-dim)", label: order.status };
          const variant = order.status as "pending" | "expired" | "filled";

          return (
            <div
              key={order.id}
              className="flex items-center justify-between"
              style={{
                padding: "var(--space-md) 0",
                borderTop: i > 0 ? "1px solid var(--border)" : "none",
              }}
            >
              <div className="flex items-center gap-[var(--space-md)]">
                <div
                  style={{
                    fontSize: "var(--text-xs)",
                    fontWeight: 600,
                    color: order.type === "buy" ? "var(--green)" : "var(--red)",
                    textTransform: "uppercase",
                    letterSpacing: "0.3px",
                    width: 28,
                  }}
                >
                  {order.type}
                </div>
                <div>
                  <div style={{ fontSize: "var(--body-size)", fontWeight: 500, color: "var(--text-primary)" }}>
                    {order.symbol}
                  </div>
                  <div style={{ fontSize: "var(--text-xs)", color: "var(--text-dim)" }}>
                    @ {order.target_price}
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-[var(--space-sm)]">
                <StatusBadge variant={variant}>{config.label}</StatusBadge>
                {order.status === "expired" && (
                  <span className="renew-link">Renew</span>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </WidgetCard>
  );
}
