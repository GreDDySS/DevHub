import React, { useState, useEffect } from "react";
import { TitleBar } from "./TitleBar";
import { Sidebar } from "./Sidebar";
import { CloseDialog } from "./CloseDialog";
import { ProjectList } from "@/components/projects/ProjectList";
import { LinkList } from "@/components/links/LinkList";
import { SettingsForm } from "@/components/settings/SettingsForm";
import { CommandPalette } from "@/components/palette/CommandPalette";
import { Toaster } from "@/components/ui/toaster";
import { useSettingsStore } from "@/stores/settingsStore";
import { useUiStore } from "@/stores/uiStore";

export function Shell() {
  const { activeView, setActiveView } = useUiStore();
  const [isSidebarExpanded, setIsSidebarExpanded] = useState(true);
  const [paletteOpen, setPaletteOpen] = useState(false);

  const fetchSettings = useSettingsStore((s) => s.fetchSettings);

  useEffect(() => {
    fetchSettings();
  }, []);

  useEffect(() => {
    const onKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.code === "KeyK") {
        e.preventDefault();
        setPaletteOpen((v) => !v);
      }
    };
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, []);

  const renderView = () => {
    switch (activeView) {
      case "projects":
        return <ProjectList />;
      case "links":
        return <LinkList />;
      case "settings":
        return <SettingsForm />;
      default:
        return <ProjectList />;
    }
  };

  return (
    <div className="flex flex-col h-screen bg-background">
      <TitleBar
        isSidebarExpanded={isSidebarExpanded}
        onToggleSidebar={() => setIsSidebarExpanded(!isSidebarExpanded)}
      />
      <div className="flex flex-1 overflow-hidden">
        <Sidebar
          activeView={activeView}
          onNavigate={setActiveView}
          isExpanded={isSidebarExpanded}
        />
        <main className="flex-1 overflow-auto p-6">{renderView()}</main>
      </div>
      <CloseDialog />
      {paletteOpen && (
        <CommandPalette onClose={() => setPaletteOpen(false)} />
      )}
      <Toaster />
    </div>
  );
}
