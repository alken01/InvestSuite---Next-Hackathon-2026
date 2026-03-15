"use client";

import { motion, AnimatePresence, useMotionValue, useTransform, PanInfo } from "framer-motion";
import type { ExpandedCardConfig, ExpandedCardSection } from "@/types/layout";
import { ActionButton } from "@/components/ui/action-button";

import { InsightSection } from "@/components/expanded-sections/InsightSection";
import { EducationSection } from "@/components/expanded-sections/EducationSection";
import { MetricsSection } from "@/components/expanded-sections/MetricsSection";
import { RatingsSection } from "@/components/expanded-sections/RatingsSection";
import { PositionSection } from "@/components/expanded-sections/PositionSection";
import { ComparisonSection } from "@/components/expanded-sections/ComparisonSection";
import { ReframeSection } from "@/components/expanded-sections/ReframeSection";
import { ContextCardSection } from "@/components/expanded-sections/ContextCardSection";

interface ExpandedCardProps {
  config: ExpandedCardConfig | undefined;
  onAction: (query: string) => void;
  onDismiss: () => void;
}

function renderSection(section: ExpandedCardSection, index: number) {
  switch (section.type) {
    case "insight":
      return <InsightSection key={index} {...section} />;
    case "education":
      return <EducationSection key={index} {...section} />;
    case "metrics":
      return <MetricsSection key={index} {...section} />;
    case "ratings":
      return <RatingsSection key={index} {...section} />;
    case "position":
      return <PositionSection key={index} {...section} />;
    case "comparison":
      return <ComparisonSection key={index} {...section} />;
    case "reframe":
      return <ReframeSection key={index} {...section} />;
    case "context_card":
      return <ContextCardSection key={index} {...section} />;
    default:
      return null;
  }
}

const DISMISS_THRESHOLD = 80;

export function ExpandedCard({ config, onAction, onDismiss }: ExpandedCardProps) {
  const dragY = useMotionValue(0);
  const cardOpacity = useTransform(dragY, [0, 200], [1, 0.3]);

  function handleDragEnd(_: unknown, info: PanInfo) {
    if (info.offset.y > DISMISS_THRESHOLD || info.velocity.y > 300) {
      onDismiss();
    }
  }

  return (
    <AnimatePresence>
      {config && (
        <>
          {/* Background overlay - tap to dismiss */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
            className="absolute inset-0 expanded-card-overlay"
            onClick={onDismiss}
          />

          <motion.div
            initial={{ y: 40, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            exit={{ y: 120, opacity: 0 }}
            transition={{ duration: 0.45, ease: [0.16, 1, 0.3, 1] }}
            drag="y"
            dragConstraints={{ top: 0, bottom: 0 }}
            dragElastic={0.6}
            onDragEnd={handleDragEnd}
            style={{ y: dragY, opacity: cardOpacity }}
            className="absolute z-20 flex flex-col glass-card hide-scrollbar expanded-card-panel"
          >
            {/* Drag handle */}
            <div className="flex justify-center pb-3">
              <div className="drag-handle" />
            </div>

            {/* Header */}
            <div className="flex items-start justify-between">
              <div>
                <span
                  className="text-white text-[length:var(--text-title)] font-medium"
                >
                  {config.name}
                </span>
              </div>
              {config.price && (
                <div className="flex items-baseline gap-2">
                  <span
                    className="font-display text-white text-[length:var(--text-title)]"
                  >
                    {config.price}
                  </span>
                  {config.price_delta && (
                    <span
                      className="font-display text-[length:var(--body-size)]"
                      style={{ color: config.price_color ?? "var(--text-mid)" }}
                    >
                      {config.price_delta}
                    </span>
                  )}
                </div>
              )}
            </div>

            {/* Subtitle */}
            {config.subtitle && (
              <span className="mt-1.5 text-[length:var(--label-size)] text-[var(--text-dim)]">
                {config.subtitle}
              </span>
            )}

            {/* Sections */}
            {config.sections && config.sections.length > 0 && (
              <div className="mt-5 flex flex-col gap-4">
                {config.sections.map((section, i) => renderSection(section, i))}
              </div>
            )}

            {/* Actions - filter out "back to home" style actions */}
            {config.actions && config.actions.filter(a => !a.query.match(/^(back to home|go back|home)$/i)).length > 0 && (
              <div className="mt-8 flex gap-3">
                {config.actions
                  .filter(a => !a.query.match(/^(back to home|go back|home)$/i))
                  .map((action, i) => (
                    <ActionButton
                      key={i}
                      variant={action.style}
                      onClick={() => onAction(action.query)}
                      className="flex-1"
                    >
                      {action.label}
                    </ActionButton>
                  ))}
              </div>
            )}

            {/* Footer */}
            {config.footer && (
              <p className="mt-4 text-center text-[length:var(--text-xs)] text-[var(--text-muted)]">
                {config.footer}
              </p>
            )}
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
}
