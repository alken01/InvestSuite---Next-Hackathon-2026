"use client";

import React from "react";
import { InsightSection } from "./InsightSection";
import { EducationSection } from "./EducationSection";
import { MetricsSection } from "./MetricsSection";
import { RatingsSection } from "./RatingsSection";
import { PositionSection } from "./PositionSection";
import { ComparisonSection } from "./ComparisonSection";
import { ReframeSection } from "./ReframeSection";
import { ContextCardSection } from "./ContextCardSection";
import type { ExpandedCardSection } from "@/types/layout";

export {
  InsightSection,
  EducationSection,
  MetricsSection,
  RatingsSection,
  PositionSection,
  ComparisonSection,
  ReframeSection,
  ContextCardSection,
};

export function SectionRenderer({ section }: { section: ExpandedCardSection }) {
  switch (section.type) {
    case "insight":
      return React.createElement(InsightSection, section);
    case "education":
      return React.createElement(EducationSection, section);
    case "metrics":
      return React.createElement(MetricsSection, section);
    case "ratings":
      return React.createElement(RatingsSection, section);
    case "position":
      return React.createElement(PositionSection, section);
    case "comparison":
      return React.createElement(ComparisonSection, section);
    case "reframe":
      return React.createElement(ReframeSection, section);
    case "context_card":
      return React.createElement(ContextCardSection, section);
    default:
      return null;
  }
}
