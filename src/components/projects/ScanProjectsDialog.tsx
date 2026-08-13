import React, { useState } from "react";
import { X, FolderSearch, Trash2, Check, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useProjectStore } from "@/stores/projectStore";
import type { Project } from "@/lib/types";
import { LANGUAGE_ICONS, LANGUAGE_COLORS } from "@/lib/types";
import { cn } from "@/lib/utils";

interface ScanProjectsDialogProps {
  onClose: () => void;
}

export function ScanProjectsDialog({ onClose }: ScanProjectsDialogProps) {
  const { addProject } = useProjectStore();
  const [projects, setProjects] = useState<Project[]>([]);
  const [scanning, setScanning] = useState(false);
  const [adding, setAdding] = useState(false);
  const [selectedDir, setSelectedDir] = useState<string>("");
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editName, setEditName] = useState("");

  const handleSelectDir = async () => {
    try {
      const { open } = await import("@tauri-apps/plugin-dialog");
      const dir = await open({
        directory: true,
        multiple: false,
        title: "Select directory to scan",
      });
      if (dir) {
        setSelectedDir(dir as string);
        setProjects([]);
      }
    } catch (e) {
      console.error("Failed to open dialog:", e);
    }
  };

  const handleScan = async () => {
    if (!selectedDir) return;
    setScanning(true);
    try {
      const { invoke } = await import("@tauri-apps/api/core");
      const detected = await invoke<Project[]>("detect_projects", {
        rootPath: selectedDir,
      });
      setProjects(detected);
    } catch (e) {
      console.error("Failed to scan:", e);
    } finally {
      setScanning(false);
    }
  };

  const handleRemove = (id: string) => {
    setProjects((prev) => prev.filter((p) => p.id !== id));
  };

  const handleStartEdit = (project: Project) => {
    setEditingId(project.id);
    setEditName(project.name);
  };

  const handleSaveEdit = (id: string) => {
    if (editName.trim()) {
      setProjects((prev) =>
        prev.map((p) => (p.id === id ? { ...p, name: editName.trim() } : p))
      );
    }
    setEditingId(null);
  };

  const handleAddAll = async () => {
    setAdding(true);
    try {
      for (const project of projects) {
        await addProject({
          name: project.name,
          path: project.path,
          language: project.language,
        });
      }
      onClose();
    } catch (e) {
      console.error("Failed to add projects:", e);
    } finally {
      setAdding(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <Card className="w-full max-w-2xl mx-4 max-h-[80vh] flex flex-col">
        <CardHeader className="flex flex-row items-center justify-between pb-3">
          <CardTitle className="text-base flex items-center gap-2">
            <FolderSearch className="h-4 w-4" />
            Scan for Projects
          </CardTitle>
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
        </CardHeader>
        <CardContent className="flex-1 overflow-hidden flex flex-col gap-4">
          {/* Directory selector */}
          <div className="flex gap-2">
            <Input
              placeholder="Select directory to scan..."
              value={selectedDir}
              readOnly
              className="flex-1"
            />
            <Button variant="outline" onClick={handleSelectDir}>
              Browse
            </Button>
            <Button
              onClick={handleScan}
              disabled={!selectedDir || scanning}
            >
              {scanning ? (
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
              ) : (
                <FolderSearch className="h-4 w-4 mr-2" />
              )}
              Scan
            </Button>
          </div>

          {/* Results */}
          {projects.length > 0 && (
            <div className="flex-1 overflow-auto space-y-2">
              <p className="text-sm text-muted-foreground">
                Found {projects.length} project{projects.length !== 1 ? "s" : ""}
              </p>
              <div className="space-y-1.5">
                {projects.map((project) => (
                  <div
                    key={project.id}
                    className="flex items-center gap-2 p-2 rounded-lg border bg-card"
                  >
                    <div
                      className={cn(
                        "flex items-center justify-center w-8 h-8 rounded-lg font-mono text-xs font-bold shrink-0",
                        LANGUAGE_COLORS[project.language]
                      )}
                    >
                      {LANGUAGE_ICONS[project.language]}
                    </div>
                    <div className="flex-1 min-w-0">
                      {editingId === project.id ? (
                        <Input
                          value={editName}
                          onChange={(e) => setEditName(e.target.value)}
                          onBlur={() => handleSaveEdit(project.id)}
                          onKeyDown={(e) => {
                            if (e.key === "Enter") handleSaveEdit(project.id);
                            if (e.key === "Escape") setEditingId(null);
                          }}
                          className="h-7 text-sm"
                          autoFocus
                        />
                      ) : (
                        <p
                          className="text-sm font-medium truncate cursor-pointer hover:underline"
                          onClick={() => handleStartEdit(project)}
                        >
                          {project.name}
                        </p>
                      )}
                      <p className="text-xs text-muted-foreground truncate">
                        {project.path}
                      </p>
                    </div>
                    <Badge variant="secondary" className="text-[10px] shrink-0">
                      {project.language}
                    </Badge>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-7 w-7 shrink-0"
                      onClick={() => handleRemove(project.id)}
                    >
                      <Trash2 className="h-3.5 w-3.5 text-muted-foreground" />
                    </Button>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Empty state */}
          {selectedDir && !scanning && projects.length === 0 && (
            <div className="flex-1 flex items-center justify-center text-muted-foreground text-sm">
              Click "Scan" to find projects in this directory
            </div>
          )}

          {/* Actions */}
          {projects.length > 0 && (
            <div className="flex justify-end gap-2 pt-2 border-t">
              <Button variant="outline" onClick={onClose}>
                Cancel
              </Button>
              <Button onClick={handleAddAll} disabled={adding}>
                {adding ? (
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                ) : (
                  <Check className="h-4 w-4 mr-2" />
                )}
                Add {projects.length} Project{projects.length !== 1 ? "s" : ""}
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
