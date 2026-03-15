import { SignIn } from "@clerk/nextjs";

export default function SignInPage() {
  return (
    <div className="fixed inset-0 flex items-center justify-center bg-[var(--bg-base,#0a0a0f)]">
      <SignIn />
    </div>
  );
}
