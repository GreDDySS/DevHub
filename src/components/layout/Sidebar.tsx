import React from "react";
import { cn } from "@/lib/utils";
import {
  FolderOpen,
  Link,
  Settings,
  Moon,
  Sun,
} from "lucide-react";
import { useSettingsStore } from "@/stores/settingsStore";

interface SidebarProps {
  activeView: string;
  onNavigate: (view: string) => void;
  isExpanded: boolean;
}

const navItems = [
  { id: "projects", label: "Projects", icon: FolderOpen },
  { id: "links", label: "Links", icon: Link },
];

export function Sidebar({
  activeView,
  onNavigate,
  isExpanded,
}: SidebarProps) {
  const { settings, toggleTheme } = useSettingsStore();

  return (
    <div
      className={cn(
        "flex flex-col h-full border-r border-border bg-sidebar transition-all duration-200",
        isExpanded ? "w-[200px]" : "w-[64px]"
      )}
    >
      <nav className="flex-1 p-2 space-y-1 mt-1">
        {navItems.map((item) => (
          <button
            key={item.id}
            onClick={() => onNavigate(item.id)}
            className={cn(
              "flex items-center w-full rounded-md transition-colors",
              isExpanded ? "px-3 py-2 gap-3" : "px-0 py-2 justify-center",
              activeView === item.id
                ? "bg-sidebar-accent text-sidebar-accent-foreground"
                : "text-sidebar-foreground hover:bg-sidebar-accent/50"
            )}
          >
            <item.icon className="h-4 w-4 shrink-0" />
            {isExpanded && <span className="text-sm">{item.label}</span>}
          </button>
        ))}
      </nav>

      <div className="p-2 border-t border-border space-y-1">
        <button
          onClick={() => onNavigate("settings")}
          className={cn(
            "flex items-center w-full rounded-md transition-colors",
            isExpanded ? "px-3 py-2 gap-3" : "px-0 py-2 justify-center",
            activeView === "settings"
              ? "bg-sidebar-accent text-sidebar-accent-foreground"
              : "text-sidebar-foreground hover:bg-sidebar-accent/50"
          )}
        >
          <Settings className="h-4 w-4 shrink-0" />
          {isExpanded && <span className="text-sm">Settings</span>}
        </button>
        <button
          onClick={toggleTheme}
          className={cn(
            "flex items-center w-full rounded-md transition-colors",
            isExpanded ? "px-3 py-2 gap-3" : "px-0 py-2 justify-center",
            "text-sidebar-foreground hover:bg-sidebar-accent/50"
          )}
        >
          {settings?.is_dark_theme ? (
            <Sun className="h-4 w-4 shrink-0" />
          ) : (
            <Moon className="h-4 w-4 shrink-0" />
          )}
          {isExpanded && (
            <span className="text-sm">
              {settings?.is_dark_theme ? "Light" : "Dark"}
            </span>
          )}
        </button>
      </div>
    </div>
  );
}
