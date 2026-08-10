import React, { useState } from "react";
import { X, FolderOpen } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useProjectStore } from "@/stores/projectStore";
import type { ProgrammingLanguage } from "@/lib/types";

interface AddProjectDialogProps {
  onClose: () => void;
}

const languages: { label: string; value: ProgrammingLanguage }[] = [
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

export function AddProjectDialog({ onClose }: AddProjectDialogProps) {
  const { addProject } = useProjectStore();
  const [name, setName] = useState("");
  const [path, setPath] = useState("");
  const [description, setDescription] = useState("");
  const [language, setLanguage] = useState<ProgrammingLanguage>("Other");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !path.trim()) {
      setError("Name and path are required");
      return;
    }

    setIsSubmitting(true);
    setError(null);

    const project = await addProject({
      name: name.trim(),
      path: path.trim(),
      description: description.trim() || undefined,
      language,
    });

    setIsSubmitting(false);

    if (project) {
      onClose();
    } else {
      setError("Failed to add project");
    }
  };

  const handleBrowseFolder = async () => {
    try {
      const { open } = await import("@tauri-apps/plugin-dialog");
      const selected = await open({
        directory: true,
        multiple: false,
        title: "Select Project Folder",
      });
      if (selected) {
        setPath(selected as string);
        // Auto-fill name from folder name
        if (!name.trim()) {
          const folderName = (selected as string).split(/[\\/]/).pop() || "";
          setName(folderName);
        }
      }
    } catch (e) {
      console.error("Failed to open folder dialog:", e);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <Card className="w-full max-w-md mx-4">
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>Add Project</CardTitle>
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Name *</label>
              <Input
                placeholder="My Project"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Path *</label>
              <div className="flex gap-2">
                <Input
                  placeholder="C:\Projects\my-project"
                  value={path}
                  onChange={(e) => setPath(e.target.value)}
                  className="flex-1"
                />
                <Button
                  type="button"
                  variant="outline"
                  size="icon"
                  onClick={handleBrowseFolder}
                >
                  <FolderOpen className="h-4 w-4" />
                </Button>
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Description</label>
              <Input
                placeholder="Optional description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Language</label>
              <select
                value={language}
                onChange={(e) => setLanguage(e.target.value as ProgrammingLanguage)}
                className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              >
                {languages.map((lang) => (
                  <option key={lang.value} value={lang.value}>
                    {lang.label}
                  </option>
                ))}
              </select>
            </div>

            {error && <p className="text-sm text-destructive">{error}</p>}

            <div className="flex justify-end gap-2">
              <Button type="button" variant="outline" onClick={onClose}>
                Cancel
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? "Adding..." : "Add Project"}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
