import { NextRequest, NextResponse } from "next/server";

const BACKEND_URL = process.env.BACKEND_URL || "http://localhost:5013";
const TIMEOUT_MS = 45_000;

export async function POST(req: NextRequest) {
  const body = await req.json();

  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), TIMEOUT_MS);

  try {
    const backendRes = await fetch(`${BACKEND_URL}/api/layout`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
      signal: controller.signal,
    });

    clearTimeout(timeout);

    if (!backendRes.ok) {
      const errorText = await backendRes.text();
      console.error(`Backend error ${backendRes.status}: ${errorText}`);
      return NextResponse.json(
        { error: "Backend request failed", detail: errorText },
        { status: backendRes.status }
      );
    }

    const data = await backendRes.json();
    return NextResponse.json(data);
  } catch (err) {
    clearTimeout(timeout);
    if (err instanceof Error && err.name === "AbortError") {
      console.error("Layout request timed out after", TIMEOUT_MS, "ms");
      return NextResponse.json(
        { error: "Request timed out. The AI is taking too long — please try again." },
        { status: 504 }
      );
    }
    console.error("Backend unreachable:", err);
    return NextResponse.json(
      { error: "Backend unreachable. Is the .NET backend running?" },
      { status: 502 }
    );
  }
}
