import React, { useState, useEffect } from "react";
import { Search, Plus, Grid, List, FolderSearch, Eye, EyeOff, Scan } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ProjectCard } from "./ProjectCard";
import { ProjectDetail } from "./ProjectDetail";
import { AddProjectDialog } from "./AddProjectDialog";
import { ScanProjectsDialog } from "./ScanProjectsDialog";
import { useProjectStore } from "@/stores/projectStore";
import type { ProjectStatus } from "@/lib/types";
import { cn } from "@/lib/utils";

const statusFilters: { label: string; value: ProjectStatus | null }[] = [
  { label: "All", value: null },
  { label: "Active", value: "Active" },
  { label: "Completed", value: "Completed" },
  { label: "Paused", value: "Paused" },
  { label: "Archived", value: "Archived" },
];

export function ProjectList() {
  const {
    projects,
    filter,
    isLoading,
    viewMode,
    setFilter,
    setViewMode,
    fetchProjects,
  } = useProjectStore();

  const [searchQuery, setSearchQuery] = useState(filter.search_query || "");
  const [showAddDialog, setShowAddDialog] = useState(false);
  const [showScanDialog, setShowScanDialog] = useState(false);
  const [showHidden, setShowHidden] = useState(false);
  const [selectedProjectId, setSelectedProjectId] = useState<string | null>(null);
  const searchTimerRef = React.useRef<ReturnType<typeof setTimeout> | null>(null);
  const didMount = React.useRef(false);

  useEffect(() => {
    fetchProjects();
  }, []);

  useEffect(() => {
    if (!didMount.current) {
      didMount.current = true;
      return;
    }
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    searchTimerRef.current = setTimeout(() => {
      setFilter({ search_query: searchQuery || undefined, show_hidden: showHidden });
      fetchProjects();
    }, 300);
    return () => {
      if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    };
  }, [searchQuery, showHidden]);

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Projects</h1>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setViewMode(viewMode === "tiles" ? "list" : "tiles")}
          >
            {viewMode === "tiles" ? (
              <List className="h-4 w-4" />
            ) : (
              <Grid className="h-4 w-4" />
            )}
          </Button>
          <Button
            variant={showHidden ? "default" : "outline"}
            size="sm"
            onClick={() => setShowHidden(!showHidden)}
            title={showHidden ? "Hide hidden projects" : "Show hidden projects"}
          >
            {showHidden ? (
              <EyeOff className="h-4 w-4" />
            ) : (
              <Eye className="h-4 w-4" />
            )}
          </Button>
          <Button size="sm" onClick={() => setShowScanDialog(true)}>
            <Scan className="h-4 w-4 mr-2" />
            Scan
          </Button>
          <Button size="sm" onClick={() => setShowAddDialog(true)}>
            <Plus className="h-4 w-4 mr-2" />
            Add Project
          </Button>
        </div>
      </div>

      <div className="flex items-center gap-4 mb-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search projects..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9"
          />
        </div>
        <div className="flex items-center gap-1">
          {statusFilters.map((sf) => (
            <Button
              key={sf.label}
              variant={filter.status === sf.value ? "default" : "ghost"}
              size="sm"
              onClick={() => { setFilter({ status: sf.value }); fetchProjects(); }}
            >
              {sf.label}
            </Button>
          ))}
        </div>
      </div>

      {showHidden && (
        <div className="mb-3 px-3 py-2 rounded-md bg-yellow-500/10 border border-yellow-500/20 text-sm text-yellow-600 dark:text-yellow-400">
          Showing hidden projects
        </div>
      )}

      {isLoading ? (
        <div className="flex-1 flex items-center justify-center text-muted-foreground">
          Loading...
        </div>
      ) : projects.length === 0 ? (
        <div className="flex-1 flex flex-col items-center justify-center text-muted-foreground">
          <FolderSearch className="h-12 w-12 mb-4" />
          <p className="text-lg">No projects found</p>
          <p className="text-sm">Add a project to get started</p>
        </div>
      ) : (
        <div
          className={
            viewMode === "tiles"
              ? "grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3"
              : "flex flex-col gap-1.5"
          }
        >
          {projects.map((project) => (
            <ProjectCard
              key={project.id}
              project={project}
              viewMode={viewMode}
              onSelect={() => setSelectedProjectId(project.id)}
            />
          ))}
        </div>
      )}

      {showAddDialog && (
        <AddProjectDialog onClose={() => setShowAddDialog(false)} />
      )}
      {showScanDialog && (
        <ScanProjectsDialog onClose={() => setShowScanDialog(false)} />
      )}
      {selectedProjectId && (() => {
        const project = projects.find((p) => p.id === selectedProjectId);
        if (!project) return null;
        return (
          <ProjectDetail
            key={project.id}
            project={project}
            onClose={() => setSelectedProjectId(null)}
          />
        );
      })()}
    </div>
  );
}
