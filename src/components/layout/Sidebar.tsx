import React from "react";
import { cn } from "@/lib/utils";
import {
  FolderOpen,
  Link,
  Settings,
  Search,
  Star,
  Moon,
  Sun,
} from "lucide-react";
import { useSettingsStore } from "@/stores/settingsStore";

interface SidebarProps {
  activeView: string;
  onNavigate: (view: string) => void;
  isExpanded: boolean;
  onToggleExpand: () => void;
}

const navItems = [
  { id: "projects", label: "Projects", icon: FolderOpen },
  { id: "links", label: "Links", icon: Link },
  { id: "settings", label: "Settings", icon: Settings },
];

export function Sidebar({
  activeView,
  onNavigate,
  isExpanded,
  onToggleExpand,
}: SidebarProps) {
  const { settings, toggleTheme } = useSettingsStore();

  return (
    <div
      className={cn(
        "flex flex-col h-full border-r border-border bg-sidebar transition-all duration-200",
        isExpanded ? "w-[200px]" : "w-[64px]"
      )}
    >
      <div className="flex items-center justify-between p-3 border-b border-border">
        {isExpanded && (
          <span className="text-sm font-semibold text-sidebar-foreground">
            DevHub
          </span>
        )}
        <button
          onClick={onToggleExpand}
          className="p-1.5 rounded-md hover:bg-sidebar-accent text-sidebar-foreground"
        >
          <Search className="h-4 w-4" />
        </button>
      </div>

      <nav className="flex-1 p-2 space-y-1">
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

      <div className="p-2 border-t border-border">
        <button
          onClick={toggleTheme}
          className={cn(
            "flex items-center w-full rounded-md transition-colors",
            isExpanded ? "px-3 py-2 gap-3" : "px-0 py-2 justify-center",
            "text-sidebar-foreground hover:bg-sidebar-accent/50"
          )}
        >
          {settings?.is_dark_theme ? (
            <Sun className="h-4 w-4" />
          ) : (
            <Moon className="h-4 w-4" />
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
