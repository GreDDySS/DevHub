import React, { useState, useEffect } from "react";
import {
  Search,
  Plus,
  Grid,
  List,
  FolderSearch,
  Eye,
  EyeOff,
  Scan,
  RefreshCw,
  ArrowDownAZ,
  ArrowUpAZ,
  ArrowUpDown,
  X,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Combobox } from "@/components/ui/combobox";
import { MultiCombobox } from "@/components/ui/multi-combobox";
import { ProjectCard } from "./ProjectCard";
import { ProjectDetail } from "./ProjectDetail";
import { AddProjectDialog } from "./AddProjectDialog";
import { ScanProjectsDialog } from "./ScanProjectsDialog";
import { useProjectStore } from "@/stores/projectStore";
import { useSettingsStore } from "@/stores/settingsStore";
import { useUiStore } from "@/stores/uiStore";
import { LANGUAGE_ICONS } from "@/lib/types";
import type { ProjectStatus, ProgrammingLanguage } from "@/lib/types";
import { cn } from "@/lib/utils";

const statusOptions = [
  { label: "All", value: "all" },
  { label: "Active", value: "Active" },
  { label: "Inactive", value: "Inactive" },
];

const languageOptions: { label: string; value: ProgrammingLanguage }[] = [
  { label: "C#", value: "CSharp" },
  { label: "Python", value: "Python" },
  { label: "Rust", value: "Rust" },
  { label: "JavaScript", value: "JavaScript" },
  { label: "TypeScript", value: "TypeScript" },
  { label: "Go", value: "Go" },
  { label: "Java", value: "Java" },
  { label: "C++", value: "Cpp" },
  { label: "Other", value: "Other" },
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
    refreshProjects,
  } = useProjectStore();

  const { settings } = useSettingsStore();
  const { detailProjectId, closeProjectDetail, openProjectDetail } = useUiStore();

  const [searchQuery, setSearchQuery] = useState(filter.search_query || "");
  const [showAddDialog, setShowAddDialog] = useState(false);
  const [showScanDialog, setShowScanDialog] = useState(false);
  const [showHidden, setShowHidden] = useState(false);
  const searchTimerRef = React.useRef<ReturnType<typeof setTimeout> | null>(
    null
  );
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
      setFilter({
        search_query: searchQuery || undefined,
        show_hidden: showHidden,
      });
      fetchProjects();
    }, 300);
    return () => {
      if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    };
  }, [searchQuery, showHidden]);

  const handleStatusChange = (value: string | null) => {
    const status =
      value && value !== "all" ? (value as ProjectStatus) : undefined;
    setFilter({ status: status ?? null });
    fetchProjects();
  };

  const handleLanguageChange = (values: string[]) => {
    const languages =
      values.length > 0 ? (values as ProgrammingLanguage[]) : undefined;
    setFilter({ languages });
    fetchProjects();
  };

  const hasActiveFilters =
    !!filter.status ||
    (filter.languages && filter.languages.length > 0) ||
    (filter.sort_by && filter.sort_by !== "name_asc") ||
    !!filter.search_query;

  const clearFilters = () => {
    setSearchQuery("");
    setShowHidden(false);
    setFilter({
      search_query: undefined,
      status: undefined,
      languages: undefined,
      sort_by: "name_asc",
      show_hidden: false,
    });
    fetchProjects();
  };

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between gap-4 mb-6 min-w-0">
        <h1 className="text-2xl font-bold shrink-0">Projects</h1>
        <div className="flex items-center gap-1 shrink-0">
          <Button
            variant="outline"
            size="icon"
            className="h-8 w-8"
            onClick={() => setViewMode(viewMode === "tiles" ? "list" : "tiles")}
            title={viewMode === "tiles" ? "List view" : "Tile view"}
          >
            {viewMode === "tiles" ? (
              <List className="h-4 w-4" />
            ) : (
              <Grid className="h-4 w-4" />
            )}
          </Button>
          <Button
            variant={showHidden ? "default" : "outline"}
            size="icon"
            className="h-8 w-8"
            onClick={() => setShowHidden(!showHidden)}
            title={showHidden ? "Hide hidden projects" : "Show hidden projects"}
          >
            {showHidden ? (
              <EyeOff className="h-4 w-4" />
            ) : (
              <Eye className="h-4 w-4" />
            )}
          </Button>
          <Button
            variant="outline"
            size="icon"
            className="h-8 w-8"
            onClick={() => refreshProjects()}
            title="Refresh projects"
          >
            <RefreshCw className="h-4 w-4" />
          </Button>
          <Button
            variant="outline"
            size="icon"
            className="h-8 w-8"
            onClick={() => setShowScanDialog(true)}
            title="Scan for projects"
          >
            <Scan className="h-4 w-4" />
          </Button>
          <Button
            size="icon"
            className="h-8 w-8"
            onClick={() => setShowAddDialog(true)}
            title="Add project"
          >
            <Plus className="h-4 w-4" />
          </Button>
        </div>
      </div>

      <div className="flex items-center gap-2 mb-4 flex-wrap">
        <div className="relative flex-1 min-w-[200px] max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search projects..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9"
          />
        </div>

        <Button
          variant={filter.sort_by ? "default" : "outline"}
          size="sm"
          className="h-8 px-2 gap-1"
          onClick={() => {
            const next = filter.sort_by === "name_asc" ? "name_desc" : "name_asc";
            setFilter({ sort_by: next });
            fetchProjects();
          }}
          title={filter.sort_by === "name_asc" ? "A→Z (click for Z→A)" : "Z→A (click for A→Z)"}
        >
          {filter.sort_by === "name_asc" ? (
            <ArrowDownAZ className="h-3.5 w-3.5" />
          ) : filter.sort_by === "name_desc" ? (
            <ArrowUpAZ className="h-3.5 w-3.5" />
          ) : (
            <ArrowUpDown className="h-3.5 w-3.5" />
          )}
        </Button>

        <div className="w-px h-5 bg-border mx-0.5" />

        {settings?.statuses_enabled && (
          <Combobox
            options={statusOptions}
            value={filter.status ?? "all"}
            onChange={handleStatusChange}
            placeholder="Status"
          />
        )}

        <MultiCombobox
          options={languageOptions.map((l) => ({
            ...l,
            icon: (
              <span className="font-mono text-xs text-muted-foreground">
                {LANGUAGE_ICONS[l.value]}
              </span>
            ),
          }))}
          values={filter.languages ?? []}
          onChange={handleLanguageChange}
          placeholder="Language"
        />

        <div className="w-px h-5 bg-border mx-0.5" />

        <Button
          variant="ghost"
          size="sm"
          className={cn(
            "h-8 w-8 p-0",
            hasActiveFilters
              ? "text-muted-foreground hover:text-foreground"
              : "text-muted-foreground/40 pointer-events-none"
          )}
          onClick={clearFilters}
          title="Clear all filters"
          disabled={!hasActiveFilters}
        >
          <X className="h-4 w-4" />
        </Button>
      </div>

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
              onSelect={() => openProjectDetail(project.id)}
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
      {detailProjectId &&
        (() => {
          const project = projects.find((p) => p.id === detailProjectId);
          if (!project) return null;
          return (
            <ProjectDetail
              key={project.id}
              project={project}
              onClose={closeProjectDetail}
            />
          );
        })()}
    </div>
  );
}
