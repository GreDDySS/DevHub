import React, { useEffect, useRef } from "react";
import {
  X,
  Maximize2,
  Minimize2,
  Star,
  ExternalLink,
  Terminal,
  Code2,
  FolderOpen,
  ArrowLeft,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useProjectStore } from "@/stores/projectStore";
import { useSettingsStore } from "@/stores/settingsStore";
import type { Project } from "@/lib/types";
import {
  LANGUAGE_ICONS,
  LANGUAGE_COLORS,
  STATUS_COLORS,
} from "@/lib/types";
import { cn } from "@/lib/utils";

interface ProjectDetailProps {
  project: Project;
  onClose: () => void;
}

export function ProjectDetail({ project, onClose }: ProjectDetailProps) {
  const { openInExplorer, openInConsole, openInIde, toggleFavorite } =
    useProjectStore();
  const { settings } = useSettingsStore();
  const overlayRef = useRef<HTMLDivElement>(null);
  const [isFullscreen, setIsFullscreen] = React.useState(false);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [onClose]);

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === overlayRef.current) onClose();
  };

  const toggleFullscreen = () => {
    setIsFullscreen((prev) => !prev);
  };

  const ides = settings?.ides ?? [];

  return (
    <div
      ref={overlayRef}
      onClick={handleBackdropClick}
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-md"
    >
      <div
        className={cn(
          "flex flex-col bg-background border border-border shadow-2xl",
          isFullscreen
            ? "w-full h-full rounded-none"
            : "w-[95vw] h-[90vh] rounded-2xl"
        )}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-border shrink-0">
          <div className="flex items-center gap-4 min-w-0">
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 shrink-0"
              onClick={onClose}
            >
              <ArrowLeft className="h-4 w-4" />
            </Button>
            <div
              className={cn(
                "flex items-center justify-center w-10 h-10 rounded-xl font-mono text-sm font-bold shrink-0",
                LANGUAGE_COLORS[project.language]
              )}
            >
              {LANGUAGE_ICONS[project.language]}
            </div>
            <div className="min-w-0">
              <h1 className="text-lg font-semibold truncate">{project.name}</h1>
              <p className="text-xs text-muted-foreground truncate">
                {project.path}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-1 shrink-0">
            <Badge className={cn("text-xs", STATUS_COLORS[project.status])}>
              {project.status}
            </Badge>
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={() => toggleFavorite(project.id)}
            >
              <Star
                className={cn(
                  "h-4 w-4",
                  project.is_favorite
                    ? "fill-yellow-500 text-yellow-500"
                    : "text-muted-foreground"
                )}
              />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={toggleFullscreen}
              title={isFullscreen ? "Exit fullscreen" : "Expand to full screen"}
            >
              {isFullscreen ? (
                <Minimize2 className="h-4 w-4" />
              ) : (
                <Maximize2 className="h-4 w-4" />
              )}
            </Button>
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={onClose}
            >
              <X className="h-4 w-4" />
            </Button>
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-auto p-6">
          <p className="text-sm text-muted-foreground">
            Project details coming soon...
          </p>
        </div>

        {/* Footer actions */}
        <div className="flex items-center justify-between px-6 py-3 border-t border-border shrink-0">
          <div className="flex items-center gap-1.5">
            {project.tags.map((tag) => (
              <Badge key={tag} variant="secondary" className="text-xs">
                {tag}
              </Badge>
            ))}
          </div>
          <div className="flex items-center gap-1">
            {ides.length > 0 && (
              <div className="relative group">
                <Button variant="outline" size="sm">
                  <Code2 className="h-3.5 w-3.5 mr-1.5" />
                  Open in IDE
                </Button>
                <div className="absolute right-0 bottom-full mb-1 hidden group-hover:block z-50 w-48 rounded-lg border border-border shadow-lg py-1 bg-background">
                  {ides.map((ide, i) => (
                    <button
                      key={i}
                      className="w-full text-left px-3 py-1.5 text-sm hover:bg-accent flex items-center gap-2 transition-colors"
                      onClick={() => openInIde(project.path, ide.path)}
                    >
                      <Code2 className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
                      <span className="truncate">{ide.name}</span>
                    </button>
                  ))}
                </div>
              </div>
            )}
            <Button
              variant="outline"
              size="sm"
              onClick={() => openInConsole(project.path)}
            >
              <Terminal className="h-3.5 w-3.5 mr-1.5" />
              Terminal
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => openInExplorer(project.path)}
            >
              <FolderOpen className="h-3.5 w-3.5 mr-1.5" />
              Explorer
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
