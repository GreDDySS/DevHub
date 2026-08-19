import React, { useEffect, useRef, useState } from "react";
import { X, Minus, LogOut } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useSettingsStore } from "@/stores/settingsStore";
import { invoke } from "@tauri-apps/api/core";

export function CloseDialog() {
  const [isOpen, setIsOpen] = useState(false);
  const [rememberChoice, setRememberChoice] = useState(false);
  const { settings, saveSettings } = useSettingsStore();

  const unlistenRef = useRef<(() => void) | null>(null);

  useEffect(() => {
    let mounted = true;
    const setup = async () => {
      try {
        const { getCurrentWindow } = await import("@tauri-apps/api/window");
        const unlisten = await getCurrentWindow().listen("show-close-dialog", () => {
          setRememberChoice(false);
          setIsOpen(true);
        });
        if (mounted) {
          unlistenRef.current = unlisten;
        } else {
          unlisten();
        }
      } catch (e) {
        console.error("Failed to setup close dialog listener:", e);
      }
    };
    setup();
    return () => {
      mounted = false;
      unlistenRef.current?.();
    };
  }, []);

  const handleMinimizeToTray = async () => {
    setIsOpen(false);
    if (rememberChoice && settings) {
      await saveSettings({ ...settings, close_action: "MinimizeToTray" });
    }
    const { getCurrentWindow } = await import("@tauri-apps/api/window");
    await getCurrentWindow().hide();
  };

  const handleExit = async () => {
    setIsOpen(false);
    if (rememberChoice && settings) {
      await saveSettings({ ...settings, close_action: "Exit" });
    }
    await invoke("force_exit");
  };

  const handleCancel = () => {
    setIsOpen(false);
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <Card className="w-full max-w-sm mx-4">
        <CardHeader className="flex flex-row items-center justify-between pb-3">
          <CardTitle className="text-base">Close DevHub?</CardTitle>
          <Button variant="ghost" size="icon" onClick={handleCancel}>
            <X className="h-4 w-4" />
          </Button>
        </CardHeader>
        <CardContent className="space-y-3">
          <Button
            variant="outline"
            className="w-full justify-start"
            onClick={handleMinimizeToTray}
          >
            <Minus className="h-4 w-4 mr-3" />
            Minimize to tray
          </Button>
          <Button
            variant="outline"
            className="w-full justify-start"
            onClick={handleExit}
          >
            <LogOut className="h-4 w-4 mr-3" />
            Exit
          </Button>
          <label className="flex items-center gap-2 text-sm text-muted-foreground cursor-pointer select-none">
            <input
              type="checkbox"
              checked={rememberChoice}
              onChange={(e) => setRememberChoice(e.target.checked)}
              className="accent-primary"
            />
            Remember my choice
          </label>
          <Button
            variant="ghost"
            className="w-full"
            onClick={handleCancel}
          >
            Cancel
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
