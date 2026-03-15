"use client";

import React from "react";

interface RatingsSectionProps {
  type: "ratings";
  items: { label: string; score: number; max: number }[];
}

export function RatingsSection({ items }: RatingsSectionProps) {
  return (
    <div className="ratings-grid">
      {(items ?? []).map((item, i) => (
        <div key={i} className="ratings-item">
          <div className="ratings-dots">
            {Array.from({ length: item.max }).map((_, di) => (
              <div
                key={di}
                className={`ratings-dot ${di < item.score ? "isFilled" : ""}`}
              />
            ))}
          </div>
          <div className="micro-label">
            {item.label}
          </div>
        </div>
      ))}
    </div>
  );
}
