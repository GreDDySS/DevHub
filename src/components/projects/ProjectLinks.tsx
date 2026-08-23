import React, { useEffect, useMemo, useRef, useState } from "react";
import { Link2, Plus, ExternalLink, Copy, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useLinkStore } from "@/stores/linkStore";
import { cn } from "@/lib/utils";

interface ProjectLinksProps {
  projectId: string;
}

export function ProjectLinks({ projectId }: ProjectLinksProps) {
  const { links, fetchLinks, addLink, deleteLink, copyUrl, openInBrowser } =
    useLinkStore();
  const [newUrl, setNewUrl] = useState("");
  const [showInput, setShowInput] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    fetchLinks();
  }, [fetchLinks]);

  useEffect(() => {
    if (showInput) {
      inputRef.current?.focus();
    }
  }, [showInput]);

  const projectLinks = useMemo(
    () =>
      links
        .filter((l) => l.project_id === projectId)
        .sort(
          (a, b) =>
            new Date(b.captured_at).getTime() -
            new Date(a.captured_at).getTime()
        ),
    [links, projectId]
  );

  const isValidUrl = (value: string) =>
    value.startsWith("http://") || value.startsWith("https://");

  const handleAdd = async () => {
    const url = newUrl.trim();
    if (!url) return;
    if (!isValidUrl(url)) {
      setError("URL must start with http:// or https://");
      return;
    }
    setError(null);
    const link = await addLink(url, { projectId });
    if (link) {
      setNewUrl("");
      inputRef.current?.focus();
    } else {
      setError("Failed to add link");
    }
  };

  const closeInput = () => {
    setShowInput(false);
    setNewUrl("");
    setError(null);
  };

  return (
    <div className="group flex flex-col gap-3">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 min-w-0">
          <Link2 className="h-4 w-4 text-muted-foreground" />
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            LINKS
          </h2>
          <span className="text-xs text-muted-foreground">
            {projectLinks.length}
          </span>
        </div>
        <Button
          variant="ghost"
          size="icon"
          className={cn(
            "h-6 w-6 transition-opacity",
            error
              ? "opacity-0 pointer-events-none"
              : "opacity-40 group-hover:opacity-100"
          )}
          onClick={() => (showInput ? closeInput() : setShowInput(true))}
          title="Add link"
        >
          <Plus className="h-3.5 w-3.5" />
        </Button>
      </div>

      {showInput && (
        <div
          className="flex flex-col gap-1"
          onBlur={(e) =>
            !e.currentTarget.contains(e.relatedTarget as Node) &&
            !newUrl.trim() &&
            closeInput()
          }
        >
          <Input
            ref={inputRef}
            value={newUrl}
            onChange={(e) => {
              setNewUrl(e.target.value);
              setError(null);
            }}
            onKeyDown={(e) => {
              if (e.key === "Enter") handleAdd();
              if (e.key === "Escape") closeInput();
            }}
            placeholder={error ?? "https://github.com/... (Enter)"}
            className={cn(
              "h-8 text-sm",
              error && "border-destructive placeholder:text-destructive"
            )}
            autoFocus
          />
        </div>
      )}

      <div className="flex flex-col gap-2 h-[280px] overflow-y-auto pr-1">
        {projectLinks.length === 0 ? (
          <div className="flex-1 flex items-center justify-center py-6">
            <p className="text-xs text-muted-foreground text-center px-4">
              {showInput
                ? "Paste a URL and press Enter."
                : "No links yet. Hover the header and press + to attach one."}
            </p>
          </div>
        ) : (
          projectLinks.map((link) => (
            <div
              key={link.id}
              onClick={() => openInBrowser(link.url)}
              className="group/item flex items-center gap-3 rounded-lg border bg-card px-3 py-2 cursor-pointer hover:bg-accent/50 transition-colors"
            >
              <ExternalLink className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />
              <div className="flex-1 min-w-0">
                <h3 className="text-sm font-medium truncate">
                  {link.title || link.url}
                </h3>
                <p className="text-xs text-muted-foreground truncate">
                  {link.url}
                </p>
              </div>
              <div
                className="flex items-center gap-1 opacity-30 group-hover/item:opacity-100 shrink-0"
                onClick={(e) => e.stopPropagation()}
              >
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-7 w-7"
                  title="Copy URL"
                  onClick={() => copyUrl(link.url)}
                >
                  <Copy className="h-3.5 w-3.5 text-muted-foreground" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-7 w-7 hover:text-destructive"
                  title="Delete"
                  onClick={() => deleteLink(link.id)}
                >
                  <Trash2 className="h-3.5 w-3.5" />
                </Button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
