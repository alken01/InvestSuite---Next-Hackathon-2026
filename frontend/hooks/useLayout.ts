"use client";

import { useState, useCallback, useRef } from "react";
import { LayoutPayload } from "@/types/layout";
import { fetchContext, fetchLayout } from "@/lib/api";

export function useLayout() {
  const [layout, setLayout] = useState<LayoutPayload | null>(null);
  const [loading, setLoading] = useState(false);    // blocks input (fast context phase only)
  const [aiLoading, setAiLoading] = useState(false); // non-blocking (slow AI phase)
  const [error, setError] = useState<string | null>(null);
  const activeRequestRef = useRef(0);

  /** Fast load — computed from data, no LLM. Shows portfolio instantly. */
  const loadContext = useCallback(async (userId: string, scenario?: string, date?: string) => {
    const requestId = ++activeRequestRef.current;
    setError(null);
    try {
      const contextLayout = await fetchContext(userId, scenario, date);
      if (requestId === activeRequestRef.current) {
        setLayout(contextLayout);
      }
    } catch (e) {
      if (requestId === activeRequestRef.current) {
        const message = e instanceof Error ? e.message : "Failed to fetch context";
        setError(message);
      }
    }
  }, []);

  /** LLM load — fetches AI-generated layout and merges ai_message + suggestions. */
  const loadAiContent = useCallback(async (userId: string, scenario?: string, date?: string) => {
    const requestId = activeRequestRef.current;
    setAiLoading(true);
    try {
      const aiLayout = await fetchLayout({ type: "initial", userId, scenario, date });
      if (requestId === activeRequestRef.current) {
        setLayout((prev) => {
          if (!prev) return aiLayout;
          return { ...prev, ai_message: aiLayout.ai_message, suggestions: aiLayout.suggestions };
        });
      }
    } catch (e) {
      if (requestId === activeRequestRef.current) {
        setError(e instanceof Error ? e.message : "AI content failed");
      }
    } finally {
      if (requestId === activeRequestRef.current) {
        setAiLoading(false);
      }
    }
  }, []);

  /** Full query — shows context instantly, then replaces with AI layout. */
  const sendQuery = useCallback(async (query: string, userId?: string, scenario?: string, date?: string) => {
    const requestId = ++activeRequestRef.current;
    setLoading(true);
    setError(null);

    // Phase 1: context (fast, ~100ms) — blocks input briefly
    if (userId) {
      try {
        const contextLayout = await fetchContext(userId, scenario, date);
        if (requestId === activeRequestRef.current) {
          setLayout((prev) => prev ?? contextLayout);
        }
      } catch {
        // Non-fatal — AI layout will replace anyway
      }
    }

    // Unblock input after context; AI phase is non-blocking
    if (requestId === activeRequestRef.current) {
      setLoading(false);
      setAiLoading(true);
    }

    // Phase 2: AI (slow, 5-30s) — input stays interactive
    try {
      const aiLayout = await fetchLayout({ type: "query", query, userId, scenario, date });
      if (requestId === activeRequestRef.current) {
        setLayout(aiLayout);
      }
    } catch (e) {
      if (requestId === activeRequestRef.current) {
        const message = e instanceof Error ? e.message : "Failed to fetch layout";
        setError(message);
      }
    } finally {
      if (requestId === activeRequestRef.current) {
        setAiLoading(false);
      }
    }
  }, []);

  return { layout, setLayout, loading, aiLoading, error, loadContext, loadAiContent, sendQuery };
}
