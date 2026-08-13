import React, { useState, useEffect } from "react";
import { TitleBar } from "./TitleBar";
import { Sidebar } from "./Sidebar";
import { CloseDialog } from "./CloseDialog";
import { ProjectList } from "@/components/projects/ProjectList";
import { LinkList } from "@/components/links/LinkList";
import { SettingsForm } from "@/components/settings/SettingsForm";
import { useSettingsStore } from "@/stores/settingsStore";

export function Shell() {
  const [activeView, setActiveView] = useState("projects");
  const [isSidebarExpanded, setIsSidebarExpanded] = useState(true);

  const fetchSettings = useSettingsStore((s) => s.fetchSettings);

  useEffect(() => {
    fetchSettings();
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
    </div>
  );
}
