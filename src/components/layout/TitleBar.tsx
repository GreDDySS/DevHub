import React, { useCallback, useEffect, useState } from "react";
import { Minus, Square, X, PanelLeftClose, PanelLeft } from "lucide-react";

interface TitleBarProps {
  isSidebarExpanded: boolean;
  onToggleSidebar: () => void;
}

export function TitleBar({ isSidebarExpanded, onToggleSidebar }: TitleBarProps) {
  const [appWindow, setAppWindow] = useState<any>(null);

  useEffect(() => {
    import("@tauri-apps/api/window").then((mod) => {
      setAppWindow(mod.getCurrentWindow());
    });
  }, []);

  const handleMinimize = useCallback(async () => {
    if (!appWindow) return;
    try {
      await appWindow.minimize();
    } catch (e) {
      console.error("Failed to minimize:", e);
    }
  }, [appWindow]);

  const handleMaximize = useCallback(async () => {
    if (!appWindow) return;
    try {
      const isMaximized = await appWindow.isMaximized();
      if (isMaximized) {
        await appWindow.unmaximize();
      } else {
        await appWindow.maximize();
      }
    } catch (e) {
      console.error("Failed to maximize:", e);
    }
  }, [appWindow]);

  const handleClose = useCallback(async () => {
    if (!appWindow) return;
    try {
      await appWindow.close();
    } catch (e) {
      console.error("Failed to close:", e);
    }
  }, [appWindow]);

  return (
    <div
      data-tauri-drag-region
      className="flex items-center h-8 bg-background border-b border-border select-none"
    >
      <button
        onClick={onToggleSidebar}
        className="flex items-center justify-center h-full px-3 hover:bg-accent"
      >
        {isSidebarExpanded ? (
          <PanelLeftClose className="h-4 w-4 text-muted-foreground" />
        ) : (
          <PanelLeft className="h-4 w-4 text-muted-foreground" />
        )}
      </button>

      <span className="text-sm font-semibold text-foreground ml-1" data-tauri-drag-region>
        DevHub
      </span>

      <div className="flex-1" data-tauri-drag-region />

      <div className="flex items-center h-full">
        <button
          onClick={handleMinimize}
          className="flex items-center justify-center h-full w-12 hover:bg-accent"
        >
          <Minus className="h-4 w-4 text-muted-foreground" />
        </button>
        <button
          onClick={handleMaximize}
          className="flex items-center justify-center h-full w-12 hover:bg-accent"
        >
          <Square className="h-3.5 w-3.5 text-muted-foreground" />
        </button>
        <button
          onClick={handleClose}
          className="flex items-center justify-center h-full w-12 hover:bg-destructive hover:text-destructive-foreground"
        >
          <X className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
}
