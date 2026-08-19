import React, { useState, useRef, useEffect, useCallback } from "react";
import { Code2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useSettingsStore } from "@/stores/settingsStore";
import { cn } from "@/lib/utils";

interface IDEDropdownProps {
  onOpenInIde: (idePath: string) => void;
}

export function IDEDropdown({ onOpenInIde }: IDEDropdownProps) {
  const { settings } = useSettingsStore();
  const [open, setOpen] = useState(false);
  const [menuAbove, setMenuAbove] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLButtonElement>(null);

  const ides = settings?.ides ?? [];
  const hasIdes = ides.length > 0;

  const updateMenuDirection = useCallback(() => {
    if (!triggerRef.current) return;
    const rect = triggerRef.current.getBoundingClientRect();
    const spaceBelow = window.innerHeight - rect.bottom;
    setMenuAbove(spaceBelow < 240);
  }, []);

  useEffect(() => {
    if (!open) return;
    const handleClickOutside = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [open]);

  return (
    <div className="relative" ref={menuRef}>
      <Button
        ref={triggerRef}
        variant="ghost"
        size="icon"
        className="h-7 w-7"
        onClick={(e) => {
          e.stopPropagation();
          if (hasIdes) {
            if (!open) updateMenuDirection();
            setOpen(!open);
          }
        }}
        title="Open in IDE"
      >
        <Code2 className="h-3.5 w-3.5" />
      </Button>
      {open && hasIdes && (
        <div
          className={cn(
            "absolute right-0 z-50 w-56 rounded-lg border border-border shadow-lg py-1",
            "bg-background",
            menuAbove ? "bottom-full mb-1" : "top-full mt-1"
          )}
        >
          <div className="px-2 py-1 text-[10px] font-medium text-muted-foreground uppercase tracking-wider border-b border-border mb-1">
            Open in IDE
          </div>
          {ides.map((ide, i) => (
            <button
              key={i}
              className="w-full text-left px-3 py-1.5 text-sm hover:bg-accent flex items-center gap-2 transition-colors"
              onClick={(e) => {
                e.stopPropagation();
                onOpenInIde(ide.path);
                setOpen(false);
              }}
            >
              <Code2 className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
              <span className="truncate">{ide.name}</span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
