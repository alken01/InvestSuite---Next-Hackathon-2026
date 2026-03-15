"use client";

import { useClerk } from "@clerk/nextjs";
import Image from "next/image";
import { useState, useRef, useEffect } from "react";

interface UserMenuProps {
  name: string;
  imageUrl?: string;
}

export function UserMenu({ name, imageUrl }: UserMenuProps) {
  const { signOut } = useClerk();
  const [open, setOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const initials = name
    .split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    if (open) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [open]);

  return (
    <div ref={menuRef} className="user-menu-wrapper">
      <button
        className="user-menu-trigger"
        onClick={() => setOpen((prev) => !prev)}
        aria-label="User menu"
      >
        {imageUrl ? (
          <Image src={imageUrl} alt={name} width={30} height={30} className="user-menu-avatar" />
        ) : (
          <span className="user-menu-initials">{initials}</span>
        )}
      </button>

      {open && (
        <div className="user-menu-dropdown">
          <div className="user-menu-header">
            {imageUrl ? (
              <Image src={imageUrl} alt={name} width={28} height={28} className="user-menu-avatar-lg" />
            ) : (
              <span className="user-menu-initials-lg">{initials}</span>
            )}
            <span className="user-menu-name">{name}</span>
          </div>
          <div className="user-menu-divider" />
          <button
            className="user-menu-item"
            onClick={() => signOut()}
          >
            Sign out
          </button>
        </div>
      )}
    </div>
  );
}
