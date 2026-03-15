# The Challenge

> Design and prototype an investing experience where the interface is never the same twice.

It reads the person: their knowledge, their history, their context. It reads the moment: the market, the time of day, the investor's state of mind. And it becomes whatever that person needs right now.

This has two parts:

## The Brain (engineering)
Build a logic engine (rule-based, ML-driven, LLM-powered, or a combination) that ingests context signals: market conditions (is it crashing?), user history (did they just lose money?), behavior patterns (are they browsing casually or researching with intent?), and time context (Monday morning vs. Sunday evening). The brain decides what the investor needs right now.

## The Face (design + frontend)
Build an interface that reshapes itself based on what the brain outputs. Different content, different layout, a different experience entirely. Components that appear and disappear. Density that increases or decreases. Tone that shifts. An interface that visually mutates to fit the moment.

> Both halves matter. A beautiful face without a brain is a static mockup. A brain without a face is an API nobody sees.

## Think About This

The adaptive experience doesn't have one form. Here are three provocations to stretch your thinking. They're meant to open doors, not define the solution space.

1. **What if the best interaction is no interaction at all?** The investor checks in, everything's fine, and three seconds later they feel confident and close the app. How do you design that?

2. **What if the app pushes back?** Someone tries to sell everything during a market panic. Instead of executing the order, the interface gives them perspective first. How does it do that without being patronizing?

3. **What if the app tells a story instead of showing a dashboard?** Someone hasn't opened the app in three months. Instead of dumping tables and charts, the experience catches them up in a way that feels human. What does that look like?

4. **What does the interface do on day four of nothing?** The investor has opened the app every day this week. Markets are flat. Nothing happened. They're still checking. What do you show someone who keeps coming back when there's nothing new?

> Your job is to decide: what should it become? For whom? In what moment? And how do you build it?

## Technical Hint: Think in Widgets

> One architecture worth considering: server-driven UI.

Your backend (the brain) evaluates context signals and returns a stream of components to render. Each response includes a bit of text (the narrative) and one or more widgets (the interactive substance). The frontend doesn't hardcode screens. It renders whatever the backend sends.

A response might look like:
```json
{
  "narrative": "Tech took a hit today. Here's how it affected your portfolio:",
  "widgets": [
    { "type": "portfolio_heatmap",
      "data": { "highlight": "technology", "change": -4.2 } },
    { "type": "historical_context",
      "data": { "event": "sector_drop", "recovery_avg_months": 6 } }
  ]
}
```

Build a small library of 5 to 6 well-designed widgets (a portfolio overview, a stock card, a historical chart, a volatility gauge, a comparison view, an explanation card). The brain picks which ones to show and how to configure them. The frontend stacks and renders them.

The frontend team builds the widget library, the backend team builds the decision logic, and together you wire them up.

> This is one approach. If you have a better architecture, go for it.

## What You Get

- **Mock data.** Two investor profiles (an anxious beginner and a hands-off veteran), their portfolios, and context signals for your brain to consume. See Part 2 of this document.
- **AI is fair game.** You can use LLM APIs for the brain, for generating UI, or however you see fit. Anthropic is a sponsor of this hackathon, so you get free Claude API credits. We'd strongly recommend using them.

## What You Deliver

### Working Prototype
A working prototype that demonstrates your adaptive experience. This can be:
- A coded prototype (web, mobile, whatever stack you choose)
- A high-fidelity Figma/design prototype with realistic interactions
- A combination

The prototype must demonstrate at least **two distinct "moments"**: the same investor in two different contexts. For example: an anxious beginner checking her portfolio on a calm Tuesday vs. the morning after a 7% market drop. Same person, same data, different experience. That is the proof that your interface actually adapts.

### 5-Minute Pitch
1. Who is this for? (Which investor, which moment, which problem?)
2. What's your design principle? (What belief drives your adaptive experience?)
3. How does it work? (Live demo of your prototype)
4. Why does this matter? (What changes for the investor?)

## One Last Thing

> The investing apps of today were designed before AI, before conversational interfaces, before anyone understood how much context matters in design. They were built for a world where the software couldn't know anything about you.
>
> That world is gone.
>
> Show us what comes next.
