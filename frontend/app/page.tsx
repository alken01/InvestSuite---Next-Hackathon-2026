"use client";

import { OnboardingFlow } from "@/components/account/OnboardingFlow";
import { AmbientBackground } from "@/components/ambient/AmbientBackground";
import { BreathingRings } from "@/components/ambient/BreathingRings";
import { FloatingBubbles } from "@/components/ambient/FloatingBubbles";
import { AIMessage } from "@/components/core/AIMessage";
import { BubbleStrip } from "@/components/core/BubbleStrip";
import { CenterContent } from "@/components/core/CenterContent";
import { ExpandedCard } from "@/components/core/ExpandedCard";
import { InputBar } from "@/components/core/InputBar";
import { UserMenu } from "@/components/core/UserMenu";
import {
  WidgetRenderer,
  WidgetSkeletons,
} from "@/components/widgets/WidgetRenderer";
import { Skeleton } from "@/components/ui/skeleton";
import { KeyMomentCards } from "@/components/time-travel/KeyMomentCards";
import { useLayout } from "@/hooks/useLayout";
import {
  createAccount,
  fetchAccountByClerkId,
  fetchKeyMoments,
} from "@/lib/api";
import { ThemeProvider } from "@/providers/ThemeProvider";
import { KeyMoment, Phase, Sentiment, ViewMode } from "@/types/layout";
import { useUser } from "@clerk/nextjs";
import { AnimatePresence, motion } from "framer-motion";
import { useCallback, useEffect, useState } from "react";
import { toast } from "sonner";

export default function Home() {
  const { layout, loading, aiLoading, error, loadContext, loadAiContent, sendQuery } =
    useLayout();
  const { user, isLoaded: isClerkLoaded } = useUser();
  const [activeUser, setActiveUser] = useState("");
  const [keyMoments, setKeyMoments] = useState<KeyMoment[]>([]);
  const [activeDate, setActiveDate] = useState<string | undefined>(undefined);
  const [phase, setPhase] = useState<Phase>(Phase.Landing);
  const [accountError, setAccountError] = useState<string | null>(null);
  const [creatingAccount, setCreatingAccount] = useState(false);

  const [authChecked, setAuthChecked] = useState(false);

  useEffect(() => {
    if (!isClerkLoaded) return;
    fetchKeyMoments()
      .then(setKeyMoments)
      .catch(() => {});
  }, [isClerkLoaded]);

  // Once Clerk is loaded, look up existing account by Clerk user ID
  useEffect(() => {
    if (!isClerkLoaded || !user || authChecked) return;

    setAuthChecked(true);

    fetchAccountByClerkId(user.id)
      .then((account) => {
        if (account) {
          setActiveUser(account.id);
          setPhase(Phase.Simulator);
          // Load context for existing user (default to "Now" — no date)
          loadContext(account.id).then(() => loadAiContent(account.id));
        } else {
          setPhase(Phase.Onboarding);
        }
      })
      .catch(() => {
        setPhase(Phase.Onboarding);
      });
  }, [isClerkLoaded, user, authChecked, loadContext, loadAiContent]);

  function handleSelectMoment(date: string | undefined) {
    setActiveDate(date);
    loadContext(activeUser, undefined, date).then(() =>
      loadAiContent(activeUser, undefined, date),
    );
  }

  async function handleOnboardingComplete(profile: {
    name: string;
    experienceLevel: string;
    riskProfile: string;
    personality: string;
    clerkUserId?: string;
  }) {
    setCreatingAccount(true);
    setAccountError(null);

    try {
      const data = await createAccount({
        ...profile,
        clerkUserId: profile.clerkUserId,
      });
      const accountId = data.id;

      setActiveUser(accountId);
      setPhase(Phase.Simulator);

      // Start at "Now" — no date
      loadContext(accountId).then(() => loadAiContent(accountId));
    } catch (err) {
      setAccountError(
        err instanceof Error ? err.message : "Account creation failed",
      );
      setPhase(Phase.Onboarding);
    } finally {
      setCreatingAccount(false);
    }
  }

  const handleQuery = useCallback(
    (query: string) => {
      sendQuery(query, activeUser, undefined, activeDate);
    },
    [sendQuery, activeUser, activeDate],
  );

  const handleBubbleTap = useCallback(
    (query: string) => {
      sendQuery(query, activeUser, undefined, activeDate);
    },
    [sendQuery, activeUser, activeDate],
  );

  const handleAction = useCallback(
    (query: string) => {
      sendQuery(query, activeUser, undefined, activeDate);
    },
    [sendQuery, activeUser, activeDate],
  );

  // Show toast when AI fails but we already have a layout (non-fatal)
  useEffect(() => {
    if (error && layout) {
      toast.error(error);
    }
  }, [error, layout]);

  const handleDismiss = useCallback(() => {
    loadContext(activeUser, undefined, activeDate).then(() =>
      loadAiContent(activeUser, undefined, activeDate),
    );
  }, [loadContext, loadAiContent, activeUser, activeDate]);

  // Loading while Clerk + account lookup resolves
  if (phase === Phase.Landing || !isClerkLoaded || !user) {
    return (
      <div className="desktop-wrapper">
        <div className="app-shell">
          <AmbientBackground sentiment={Sentiment.Calm} />
          <div className="center-zone flex flex-col items-center justify-center">
            <Skeleton className="h-10 w-40 rounded-lg bg-[var(--surface)]" />
          </div>
        </div>
      </div>
    );
  }

  // Onboarding flow (new Clerk user, no account yet)
  if (phase === Phase.Onboarding) {
    const clerkName =
      [user.firstName, user.lastName].filter(Boolean).join(" ") || "Investor";
    return (
      <div className="desktop-wrapper">
        <div className="app-shell">
          <AmbientBackground sentiment={Sentiment.Calm} />
          <div className="center-zone flex flex-col items-center justify-center">
            <OnboardingFlow
              name={clerkName}
              clerkUserId={user.id}
              onComplete={handleOnboardingComplete}
            />
            {accountError && (
              <p className="text-sm text-red-400 mt-4">{accountError}</p>
            )}
          </div>
        </div>
      </div>
    );
  }

  if (!layout && !error) {
    return (
      <div className="desktop-wrapper">
        <div className="app-shell skeleton-shell">
          <div className="center-zone flex flex-col items-center gap-[var(--space-md)]">
            <Skeleton className="h-10 w-40 rounded-lg bg-[var(--surface)]" />
            <Skeleton className="h-6 w-56 rounded-md bg-[var(--surface)] opacity-60" />
            <Skeleton className="h-4 w-48 rounded-md mt-4 bg-[var(--surface)] opacity-40" />
          </div>
          <div className="absolute inset-0 pointer-events-none">
            {[
              { top: "58%", left: "20%", size: 68 },
              { top: "68%", left: "75%", size: 58 },
              { top: "78%", left: "48%", size: 50 },
              { top: "62%", left: "85%", size: 42 },
              { top: "72%", left: "12%", size: 36 },
            ].map((b, i) => (
              <Skeleton
                key={i}
                className="absolute rounded-full"
                style={{
                  top: b.top,
                  left: b.left,
                  width: b.size,
                  height: b.size,
                  background: "var(--surface)",
                  opacity: 0.3 + i * 0.08,
                }}
              />
            ))}
          </div>
          <div className="skeleton-input">
            <Skeleton className="h-12 w-full rounded-full bg-[var(--surface)]" />
          </div>
        </div>
      </div>
    );
  }

  if (error && !layout) {
    return (
      <div className="desktop-wrapper">
        <div className="app-shell error-shell">
          <div className="error-content">
            <div className="text-base font-semibold mb-[var(--space-md)]">
              Cannot reach backend
            </div>
            <div className="text-sm opacity-50 mb-[var(--space-lg)]">
              {error}
            </div>
            <button
              onClick={() => loadContext(activeUser, undefined, activeDate)}
              className="error-retry-btn"
            >
              Retry
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (!layout) return null;

  const isAmbient = layout.view_mode === ViewMode.Ambient;
  const isExpandedCard = layout.view_mode === ViewMode.ExpandedCard;
  const showStrip =
    layout.view_mode === ViewMode.Research ||
    layout.view_mode === ViewMode.BuyFlow ||
    layout.view_mode === ViewMode.SellFlow;
  const showWidgets = showStrip;

  return (
    <>
      <div className="desktop-wrapper">
        <ThemeProvider tone={layout.tone}>
          <div className="app-shell">
            <AmbientBackground sentiment={layout.sentiment} />

            {/* Greeting — top left */}
            <div className="absolute top-[68px] left-[28px] z-50">
              <span className="greeting-tag">
                Hi, {user.firstName || "There"}
              </span>
            </div>

            {/* User menu */}
            <div className="absolute top-[52px] right-[var(--space-md)] z-50">
              <UserMenu
                name={
                  [user.firstName, user.lastName].filter(Boolean).join(" ") ||
                  "User"
                }
                imageUrl={user.imageUrl}
              />
            </div>

            <BreathingRings sentiment={layout.sentiment} visible={isAmbient} />

            {/* Key moments bar (simulator mode) */}
            {keyMoments.length > 0 && phase === Phase.Simulator && (
              <KeyMomentCards
                moments={keyMoments}
                activeDate={activeDate}
                onSelect={handleSelectMoment}
                disabled={loading}
              />
            )}

            {/* ── Ambient / Expanded Card: center zone ── */}
            <AnimatePresence>
              {(isAmbient || isExpandedCard) && (
                <motion.div
                  className="center-zone"
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                  transition={{ duration: 0.4 }}
                >
                  {loading ? (
                    <div className="flex flex-col items-center gap-3 pointer-events-none">
                      <Skeleton className="h-14 w-36 rounded-lg bg-[var(--surface)]" />
                      <Skeleton className="h-4 w-24 rounded bg-[var(--surface)] opacity-60" />
                    </div>
                  ) : (
                    layout.center && (
                      <CenterContent
                        content={layout.center}
                        visible={true}
                        compact={isExpandedCard}
                      />
                    )
                  )}

                  {isAmbient && layout.ai_message && !loading && !aiLoading && (
                    <AIMessage message={layout.ai_message} visible={true} />
                  )}

                  {/* AI message skeleton while loading */}
                  {isAmbient && (loading || aiLoading) && (
                    <motion.div
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      className="flex flex-col items-center gap-2 pointer-events-none max-w-[85%]"
                    >
                      <Skeleton className="h-4 w-52 rounded bg-[var(--surface-hover)]" />
                      <Skeleton className="h-4 w-40 rounded bg-[var(--surface-hover)]" />
                      <Skeleton className="h-3 w-24 rounded bg-[var(--surface-hover)] mt-1 opacity-60" />
                    </motion.div>
                  )}
                </motion.div>
              )}
            </AnimatePresence>

            {/* Floating bubbles */}
            <AnimatePresence>
              {(isAmbient || isExpandedCard) && layout.bubbles && (
                <FloatingBubbles
                  bubbles={layout.bubbles}
                  onBubbleTap={handleBubbleTap}
                />
              )}
            </AnimatePresence>

            {/* Expanded card */}
            <AnimatePresence>
              {isExpandedCard && layout.expanded_card && (
                <ExpandedCard
                  config={layout.expanded_card}
                  onAction={handleAction}
                  onDismiss={handleDismiss}
                />
              )}
            </AnimatePresence>

            {/* Bubble strip (research + buy modes) */}
            <AnimatePresence>
              {showStrip && layout.bubble_strip && (
                <motion.div
                  initial={{ opacity: 0, y: -20 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -20 }}
                  transition={{ duration: 0.4 }}
                >
                  <BubbleStrip
                    items={layout.bubble_strip}
                    onTap={handleBubbleTap}
                  />
                </motion.div>
              )}
            </AnimatePresence>

            {/* Widget area (research + buy modes) */}
            <AnimatePresence>
              {showWidgets && layout.widgets && !aiLoading && (
                <motion.div
                  className="absolute left-0 right-0 overflow-y-auto hide-scrollbar z-[35] top-[var(--widget-area-top)] bottom-[var(--widget-area-bottom)] px-[var(--app-px)]"
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                  transition={{ duration: 0.3 }}
                >
                  {layout.ai_message && (
                    <div className="mb-[var(--space-md)]">
                      <AIMessage message={layout.ai_message} visible={true} />
                    </div>
                  )}
                  <WidgetRenderer
                    widgets={layout.widgets}
                    accountId={activeUser}
                    date={activeDate}
                    onTradeComplete={handleDismiss}
                    onQuery={handleQuery}
                  />
                </motion.div>
              )}
            </AnimatePresence>

            {/* Widget skeletons while loading in research/buy modes */}
            <AnimatePresence>
              {showWidgets && (loading || aiLoading) && (
                <motion.div
                  className="absolute left-0 right-0 overflow-y-auto hide-scrollbar z-[35] top-[var(--widget-area-top)] bottom-[var(--widget-area-bottom)] px-[var(--app-px)]"
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                  transition={{ duration: 0.2 }}
                >
                  <WidgetSkeletons />
                </motion.div>
              )}
            </AnimatePresence>


            <InputBar
              placeholder={layout.input_placeholder}
              suggestions={layout.suggestions}
              onSubmit={handleQuery}
              onChipTap={handleQuery}
              loading={loading}
              aiLoading={aiLoading}
              hideChips={showWidgets || isExpandedCard}
            />
          </div>
        </ThemeProvider>
      </div>
    </>
  );
}
