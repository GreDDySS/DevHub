import React, { useState, useRef, useEffect, useCallback } from "react";
import {
  Star,
  EyeOff,
  Eye,
  Terminal,
  Code2,
  FolderOpen,
  Trash2,
  Copy,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  ContextMenu,
  ContextMenuTrigger,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuSeparator,
  ContextMenuSub,
} from "@/components/ui/context-menu";
import { useProjectStore } from "@/stores/projectStore";
import { useSettingsStore } from "@/stores/settingsStore";
import type { Project } from "@/lib/types";
import {
  LANGUAGE_ICONS,
  LANGUAGE_COLORS,
  STATUS_COLORS,
} from "@/lib/types";
import { cn } from "@/lib/utils";

interface ProjectCardProps {
  project: Project;
  viewMode: "tiles" | "list";
  onSelect: () => void;
}

export function ProjectCard({ project, viewMode, onSelect }: ProjectCardProps) {
  const {
    toggleFavorite,
    toggleHidden,
    openInExplorer,
    openInConsole,
    openInIde,
    deleteProject,
  } = useProjectStore();
  const { settings } = useSettingsStore();
  const [ideMenuOpen, setIdeMenuOpen] = useState(false);
  const [menuAbove, setMenuAbove] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLButtonElement>(null);

  const updateMenuDirection = useCallback(() => {
    if (!triggerRef.current) return;
    const rect = triggerRef.current.getBoundingClientRect();
    const spaceBelow = window.innerHeight - rect.bottom;
    setMenuAbove(spaceBelow < 240);
  }, []);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setIdeMenuOpen(false);
      }
    };
    if (ideMenuOpen) {
      document.addEventListener("mousedown", handleClickOutside);
      return () => document.removeEventListener("mousedown", handleClickOutside);
    }
  }, [ideMenuOpen]);

  const handleCardClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    onSelect();
  };

  const handleOpenInConsole = (e: React.MouseEvent) => {
    e.stopPropagation();
    openInConsole(project.path);
  };

  const handleOpenInIde = (e: React.MouseEvent, idePath: string) => {
    e.stopPropagation();
    openInIde(project.path, idePath);
    setIdeMenuOpen(false);
  };

  const handleToggleFavorite = (e: React.MouseEvent) => {
    e.stopPropagation();
    toggleFavorite(project.id);
  };

  const handleToggleHidden = (e: React.MouseEvent) => {
    e.stopPropagation();
    toggleHidden(project.id);
  };

  const ides = settings?.ides ?? [];
  const hasIdes = ides.length > 0;

  const IDEDropdown = () => (
    <div className="relative" ref={menuRef}>
      <Button
        ref={triggerRef}
        variant="ghost"
        size="icon"
        className="h-7 w-7"
        onClick={(e) => {
          e.stopPropagation();
          if (hasIdes) {
            if (!ideMenuOpen) updateMenuDirection();
            setIdeMenuOpen(!ideMenuOpen);
          }
        }}
        title="Open in IDE"
      >
        <Code2 className="h-3.5 w-3.5" />
      </Button>
      {ideMenuOpen && hasIdes && (
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
              onClick={(e) => handleOpenInIde(e, ide.path)}
            >
              <Code2 className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
              <span className="truncate">{ide.name}</span>
            </button>
          ))}
        </div>
      )}
    </div>
  );

  if (viewMode === "list") {
    return (
      <ContextMenu>
        <ContextMenuTrigger>
          <div
            className={cn(
              "group flex items-center gap-3 px-4 py-2.5 rounded-lg border bg-card hover:bg-accent/50 cursor-pointer transition-all duration-150",
              project.is_hidden && "opacity-50"
            )}
            onClick={handleCardClick}
          >
            <div
              className={cn(
                "flex items-center justify-center w-9 h-9 rounded-lg font-mono text-xs font-bold shrink-0",
                LANGUAGE_COLORS[project.language]
              )}
            >
              {LANGUAGE_ICONS[project.language]}
            </div>

            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2">
                <span className="font-medium truncate text-sm">{project.name}</span>
                <Badge className={cn("text-[10px] px-1.5 py-0", STATUS_COLORS[project.status])}>
                  {project.status}
                </Badge>
                {project.is_hidden && (
                  <Badge variant="outline" className="text-[10px] px-1.5 py-0 text-muted-foreground">
                    Hidden
                  </Badge>
                )}
              </div>
              <p className="text-xs text-muted-foreground truncate mt-0.5">
                {project.path}
              </p>
            </div>

            <div className="flex items-center gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity">
              <IDEDropdown />
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7"
                onClick={handleOpenInConsole}
                title="Open in terminal"
              >
                <Terminal className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7"
                onClick={handleToggleFavorite}
              >
                <Star
                  className={cn(
                    "h-3.5 w-3.5",
                    project.is_favorite
                      ? "fill-yellow-500 text-yellow-500"
                      : "text-muted-foreground"
                  )}
                />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7"
                onClick={handleToggleHidden}
              >
                {project.is_hidden ? (
                  <Eye className="h-3.5 w-3.5 text-muted-foreground" />
                ) : (
                  <EyeOff className="h-3.5 w-3.5 text-muted-foreground" />
                )}
              </Button>
            </div>
          </div>
        </ContextMenuTrigger>
        <ContextMenuContent>
          <ContextMenuItem onClick={() => openInExplorer(project.path)}>
            <FolderOpen className="h-4 w-4" />
            Open in Explorer
          </ContextMenuItem>
          <ContextMenuItem onClick={() => openInConsole(project.path)}>
            <Terminal className="h-4 w-4" />
            Open in Terminal
          </ContextMenuItem>
          {ides.length > 0 && (
            <ContextMenuSub label="Open in IDE">
              {ides.map((ide, i) => (
                <ContextMenuItem key={i} onClick={() => openInIde(project.path, ide.path)}>
                  <Code2 className="h-4 w-4" />
                  {ide.name}
                </ContextMenuItem>
              ))}
            </ContextMenuSub>
          )}
          <ContextMenuSeparator />
          <ContextMenuItem onClick={() => toggleFavorite(project.id)}>
            <Star className={cn("h-4 w-4", project.is_favorite && "fill-yellow-500 text-yellow-500")} />
            {project.is_favorite ? "Unfavorite" : "Favorite"}
          </ContextMenuItem>
          <ContextMenuItem onClick={() => toggleHidden(project.id)}>
            {project.is_hidden ? <Eye className="h-4 w-4" /> : <EyeOff className="h-4 w-4" />}
            {project.is_hidden ? "Unhide" : "Hide"}
          </ContextMenuItem>
          <ContextMenuSeparator />
          <ContextMenuItem
            onClick={() => navigator.clipboard.writeText(project.path)}
          >
            <Copy className="h-4 w-4" />
            Copy path
          </ContextMenuItem>
          <ContextMenuItem destructive onClick={() => deleteProject(project.id)}>
            <Trash2 className="h-4 w-4" />
            Delete
          </ContextMenuItem>
        </ContextMenuContent>
      </ContextMenu>
    );
  }

  return (
    <ContextMenu>
      <ContextMenuTrigger>
        <div
          className={cn(
            "group relative flex flex-col rounded-xl border bg-card hover:shadow-md hover:border-accent-foreground/20 cursor-pointer transition-all duration-150",
            project.is_hidden && "opacity-50"
          )}
          onClick={handleCardClick}
        >
          <div className="flex items-start justify-between p-4 pb-2">
            <div className="flex items-center gap-3">
              <div
                className={cn(
                  "flex items-center justify-center w-11 h-11 rounded-xl font-mono text-sm font-bold shrink-0",
                  LANGUAGE_COLORS[project.language]
                )}
              >
                {LANGUAGE_ICONS[project.language]}
              </div>
              <div className="min-w-0">
                <h3 className="font-semibold text-sm truncate">{project.name}</h3>
                <p className="text-xs text-muted-foreground truncate max-w-[180px] mt-0.5">
                  {project.path}
                </p>
              </div>
            </div>

            <div className="flex items-center gap-0.5 shrink-0">
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 opacity-0 group-hover:opacity-100 transition-opacity"
                onClick={handleToggleFavorite}
              >
                <Star
                  className={cn(
                    "h-3.5 w-3.5",
                    project.is_favorite
                      ? "fill-yellow-500 text-yellow-500"
                      : "text-muted-foreground"
                  )}
                />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 opacity-0 group-hover:opacity-100 transition-opacity"
                onClick={handleToggleHidden}
              >
                {project.is_hidden ? (
                  <Eye className="h-3.5 w-3.5 text-muted-foreground" />
                ) : (
                  <EyeOff className="h-3.5 w-3.5 text-muted-foreground" />
                )}
              </Button>
            </div>
          </div>

          {project.description && (
            <p className="px-4 pb-2 text-xs text-muted-foreground line-clamp-2">
              {project.description}
            </p>
          )}

          <div className="mt-auto px-4 pb-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-1.5">
                <Badge
                  className={cn("text-[10px] px-1.5 py-0", STATUS_COLORS[project.status])}
                >
                  {project.status}
                </Badge>
                {project.tags.slice(0, 2).map((tag) => (
                  <Badge
                    key={tag}
                    variant="secondary"
                    className="text-[10px] px-1.5 py-0"
                  >
                    {tag}
                  </Badge>
                ))}
                {project.tags.length > 2 && (
                  <span className="text-[10px] text-muted-foreground">
                    +{project.tags.length - 2}
                  </span>
                )}
              </div>

              <div className="flex items-center gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity">
                <IDEDropdown />
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-7 w-7"
                  onClick={handleOpenInConsole}
                  title="Open in terminal"
                >
                  <Terminal className="h-3.5 w-3.5" />
                </Button>
              </div>
            </div>
          </div>
        </div>
      </ContextMenuTrigger>
      <ContextMenuContent>
        <ContextMenuItem onClick={() => openInExplorer(project.path)}>
          <FolderOpen className="h-4 w-4" />
          Open in Explorer
        </ContextMenuItem>
        <ContextMenuItem onClick={() => openInConsole(project.path)}>
          <Terminal className="h-4 w-4" />
          Open in Terminal
        </ContextMenuItem>
        {ides.length > 0 && (
          <ContextMenuSub label="Open in IDE">
            {ides.map((ide, i) => (
              <ContextMenuItem key={i} onClick={() => openInIde(project.path, ide.path)}>
                <Code2 className="h-4 w-4" />
                {ide.name}
              </ContextMenuItem>
            ))}
          </ContextMenuSub>
        )}
        <ContextMenuSeparator />
        <ContextMenuItem onClick={() => toggleFavorite(project.id)}>
          <Star className={cn("h-4 w-4", project.is_favorite && "fill-yellow-500 text-yellow-500")} />
          {project.is_favorite ? "Unfavorite" : "Favorite"}
        </ContextMenuItem>
        <ContextMenuItem onClick={() => toggleHidden(project.id)}>
          {project.is_hidden ? <Eye className="h-4 w-4" /> : <EyeOff className="h-4 w-4" />}
          {project.is_hidden ? "Unhide" : "Hide"}
        </ContextMenuItem>
        <ContextMenuSeparator />
        <ContextMenuItem
          onClick={() => navigator.clipboard.writeText(project.path)}
        >
          <Copy className="h-4 w-4" />
          Copy path
        </ContextMenuItem>
        <ContextMenuItem destructive onClick={() => deleteProject(project.id)}>
          <Trash2 className="h-4 w-4" />
          Delete
        </ContextMenuItem>
      </ContextMenuContent>
    </ContextMenu>
  );
}
