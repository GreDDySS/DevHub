import React from "react";
import { Star, EyeOff, Eye, ExternalLink, Terminal } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useProjectStore } from "@/stores/projectStore";
import type { Project } from "@/lib/types";
import { LANGUAGE_ICONS, STATUS_COLORS } from "@/lib/types";
import { cn } from "@/lib/utils";

interface ProjectCardProps {
  project: Project;
  viewMode: "tiles" | "list";
}

export function ProjectCard({ project, viewMode }: ProjectCardProps) {
  const { toggleFavorite, toggleHidden, openInExplorer, openInConsole } =
    useProjectStore();

  const handleOpenInExplorer = (e: React.MouseEvent) => {
    e.stopPropagation();
    openInExplorer(project.path);
  };

  const handleOpenInConsole = (e: React.MouseEvent) => {
    e.stopPropagation();
    openInConsole(project.path);
  };

  const handleToggleFavorite = (e: React.MouseEvent) => {
    e.stopPropagation();
    toggleFavorite(project.id);
  };

  const handleToggleHidden = (e: React.MouseEvent) => {
    e.stopPropagation();
    toggleHidden(project.id);
  };

  if (viewMode === "list") {
    return (
      <div
        className={cn(
          "flex items-center gap-4 p-3 rounded-lg border bg-card hover:bg-accent/50 cursor-pointer transition-colors",
          project.is_hidden && "opacity-60"
        )}
        onClick={handleOpenInExplorer}
      >
        <div className="flex items-center justify-center w-10 h-10 rounded-md bg-secondary text-secondary-foreground font-mono text-sm font-bold">
          {LANGUAGE_ICONS[project.language]}
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <h3 className="font-medium truncate">{project.name}</h3>
            {project.is_hidden && (
              <Badge variant="outline" className="text-xs text-muted-foreground">
                Hidden
              </Badge>
            )}
            <Badge className={cn("text-xs", STATUS_COLORS[project.status])}>
              {project.status}
            </Badge>
          </div>
          <p className="text-sm text-muted-foreground truncate">
            {project.path}
          </p>
        </div>
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={handleToggleFavorite}
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
            onClick={handleOpenInConsole}
          >
            <Terminal className="h-4 w-4 text-muted-foreground" />
          </Button>
        </div>
      </div>
    );
  }

  return (
    <Card
      className={cn(
        "hover:bg-accent/50 cursor-pointer transition-colors",
        project.is_hidden && "opacity-60"
      )}
      onClick={handleOpenInExplorer}
    >
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center w-10 h-10 rounded-md bg-secondary text-secondary-foreground font-mono text-sm font-bold">
              {LANGUAGE_ICONS[project.language]}
            </div>
            <div>
              <CardTitle className="text-base">{project.name}</CardTitle>
              <p className="text-xs text-muted-foreground truncate max-w-[200px]">
                {project.path}
              </p>
            </div>
          </div>
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={handleToggleFavorite}
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
              onClick={handleToggleHidden}
            >
              {project.is_hidden ? (
                <Eye className="h-4 w-4 text-muted-foreground" />
              ) : (
                <EyeOff className="h-4 w-4 text-muted-foreground" />
              )}
            </Button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-1">
            {project.is_hidden && (
              <Badge variant="outline" className="text-xs text-muted-foreground">
                Hidden
              </Badge>
            )}
            <Badge className={cn("text-xs", STATUS_COLORS[project.status])}>
              {project.status}
            </Badge>
          </div>
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={handleOpenInConsole}
            >
              <Terminal className="h-4 w-4 text-muted-foreground" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={handleOpenInExplorer}
            >
              <ExternalLink className="h-4 w-4 text-muted-foreground" />
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
