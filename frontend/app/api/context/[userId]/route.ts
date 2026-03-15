import { NextRequest, NextResponse } from "next/server";

const BACKEND_URL = process.env.BACKEND_URL || "http://localhost:5013";

export async function GET(
  req: NextRequest,
  { params }: { params: Promise<{ userId: string }> }
) {
  const { userId } = await params;
  const scenario = req.nextUrl.searchParams.get("scenario") ?? "";
  const date = req.nextUrl.searchParams.get("date") ?? "";
  const qsParts: string[] = [];
  if (scenario) qsParts.push(`scenario=${encodeURIComponent(scenario)}`);
  if (date) qsParts.push(`date=${encodeURIComponent(date)}`);
  const qs = qsParts.length > 0 ? `?${qsParts.join("&")}` : "";

  try {
    const backendRes = await fetch(
      `${BACKEND_URL}/api/context/${encodeURIComponent(userId)}${qs}`
    );

    if (!backendRes.ok) {
      const errorText = await backendRes.text();
      return NextResponse.json(
        { error: "Backend error", detail: errorText },
        { status: backendRes.status }
      );
    }

    const data = await backendRes.json();
    return NextResponse.json(data);
  } catch {
    return NextResponse.json(
      { error: "Backend unreachable. Is the .NET backend running?" },
      { status: 502 }
    );
  }
}
