import { NextResponse } from "next/server";

const BACKEND_URL = process.env.BACKEND_URL || "http://localhost:5013";

export async function GET() {
  try {
    const backendRes = await fetch(`${BACKEND_URL}/api/accounts`);

    if (!backendRes.ok) {
      return NextResponse.json([], { status: backendRes.status });
    }

    const data = await backendRes.json();
    return NextResponse.json(data);
  } catch {
    console.error("Failed to fetch users from backend");
    return NextResponse.json([], { status: 502 });
  }
}
