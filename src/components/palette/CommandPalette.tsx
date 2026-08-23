import React, { useEffect, useMemo, useRef, useState } from "react";
import {
  Search,
  Code2,
  Terminal,
  FolderOpen,
  Copy,
  ExternalLink,
  Link2,
  Info,
} from "lucide-react";
import { Input } from "@/components/ui/input";
import { useProjectStore } from "@/stores/projectStore";
import { useLinkStore } from "@/stores/linkStore";
import { useSettingsStore } from "@/stores/settingsStore";
import { useUiStore } from "@/stores/uiStore";
import type { Project, Link } from "@/lib/types";
import { LANGUAGE_ICONS } from "@/lib/types";
import { cn, getPaletteHotkeyLabel } from "@/lib/utils";
import { toast } from "@/stores/toastStore";
import { fuzzyMatch, type FuzzyResult } from "@/lib/fuzzy";

interface PaletteAction {
  id: string;
  label: string;
  icon: React.ReactNode;
  run: () => void;
}

interface PaletteItem {
  id: string;
  title: string;
  subtitle?: string;
  icon: React.ReactNode;
  keywords: string;
  primary: PaletteAction;
  actions: PaletteAction[];
}

interface MatchedItem {
  item: PaletteItem;
  result: FuzzyResult;
  highlightInTitle: boolean;
}

async function copyText(text: string) {
  try {
    const { writeText } = await import("@tauri-apps/plugin-clipboard-manager");
    await writeText(text);
    toast.success("Copied to clipboard");
  } catch (e) {
    toast.error(String(e));
  }
}

function HighlightedText({ text, indices }: { text: string; indices: number[] }) {
  if (indices.length === 0) return <>{text}</>;
  const set = new Set(indices);
  return (
    <>
      {text.split("").map((ch, i) =>
        set.has(i) ? (
          <span key={i} className="text-primary font-medium">
            {ch}
          </span>
        ) : (
          <React.Fragment key={i}>{ch}</React.Fragment>
        )
      )}
    </>
  );
}

function buildProjectItems(
  projects: Project[],
  settings: ReturnType<typeof useSettingsStore.getState>["settings"],
  openProjectDetail: (projectId: string) => void
): PaletteItem[] {
  const ides = settings?.ides ?? [];

  return projects.map((project) => {
    const preferred =
      ides.find((i) => i.name === project.preferred_ide) ??
      ides[settings?.default_ide_index ?? 0] ??
      ides[0];

    const details: PaletteAction = {
      id: "details",
      label: "Details",
      icon: <Info className="h-3.5 w-3.5" />,
      run: () => openProjectDetail(project.id),
    };

    const openIde = (path: string) => ({
      id: `ide-${path}`,
      label: `Open in ${ideName(path)}`,
      icon: <Code2 className="h-3.5 w-3.5" />,
      run: async () => {
        const { invoke } = await import("@tauri-apps/api/core");
        await invoke("open_in_ide", { projectPath: project.path, idePath: path });
      },
    });

    function ideName(path: string) {
      return ides.find((i) => i.path === path)?.name ?? "IDE";
    }

    const terminal: PaletteAction = {
      id: "terminal",
      label: "Terminal",
      icon: <Terminal className="h-3.5 w-3.5" />,
      run: async () => {
        const { invoke } = await import("@tauri-apps/api/core");
        await invoke("open_in_console", { path: project.path });
      },
    };

    const explorer: PaletteAction = {
      id: "explorer",
      label: "Explorer",
      icon: <FolderOpen className="h-3.5 w-3.5" />,
      run: async () => {
        const { invoke } = await import("@tauri-apps/api/core");
        await invoke("open_in_explorer", { path: project.path });
      },
    };

    const copyPath: PaletteAction = {
      id: "copy-path",
      label: "Copy path",
      icon: <Copy className="h-3.5 w-3.5" />,
      run: () => copyText(project.path),
    };

    const otherIdes = ides
      .filter((i) => i.path !== preferred?.path)
      .slice(0, 1)
      .map((i) => openIde(i.path));

    return {
      id: `project-${project.id}`,
      title: project.name,
      subtitle: project.path,
      icon: (
        <span className="flex items-center justify-center h-6 w-6 rounded-md text-[10px] font-mono font-bold bg-secondary shrink-0">
          {LANGUAGE_ICONS[project.language]}
        </span>
      ),
      keywords: `${project.tags.join(" ")} ${project.description}`,
      primary: details,
      actions: [
        details,
        ...(preferred ? [openIde(preferred.path)] : []),
        ...otherIdes,
        terminal,
        explorer,
        copyPath,
      ],
    };
  });
}

function buildLinkItems(links: Link[]): PaletteItem[] {
  return links.map((link) => {
    const open: PaletteAction = {
      id: "open",
      label: "Open in browser",
      icon: <ExternalLink className="h-3.5 w-3.5" />,
      run: async () => {
        const { invoke } = await import("@tauri-apps/api/core");
        await invoke("open_in_browser", { url: link.url });
      },
    };
    const copy: PaletteAction = {
      id: "copy-url",
      label: "Copy URL",
      icon: <Copy className="h-3.5 w-3.5" />,
      run: () => copyText(link.url),
    };
    return {
      id: `link-${link.id}`,
      title: link.title || link.url,
      subtitle: link.url,
      icon: <Link2 className="h-4 w-4 text-muted-foreground shrink-0" />,
      keywords: "",
      primary: open,
      actions: [open, copy],
    };
  });
}

interface CommandPaletteProps {
  open: boolean;
  onClose: () => void;
}

export function CommandPalette({ open, onClose }: CommandPaletteProps) {
  const { projects, fetchProjects } = useProjectStore();
  const { links, fetchLinks } = useLinkStore();
  const settings = useSettingsStore((s) => s.settings);
  const openProjectDetail = useUiStore((s) => s.openProjectDetail);

  const [query, setQuery] = useState("");
  const [selected, setSelected] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (open) {
      setQuery("");
      setSelected(0);
      fetchProjects();
      fetchLinks();
      setTimeout(() => inputRef.current?.focus(), 0);
    }
  }, [open, fetchProjects, fetchLinks]);

  const items = useMemo(
    () => [
      ...buildProjectItems(projects, settings, openProjectDetail),
      ...buildLinkItems(links),
    ],
    [projects, links, settings, openProjectDetail]
  );

  const matched = useMemo<MatchedItem[]>(() => {
    const q = query.trim();
    if (!q) {
      return items.map((item) => ({ item, result: { score: 0, indices: [] }, highlightInTitle: false }));
    }
    const results: MatchedItem[] = [];
    for (const item of items) {
      const titleResult = fuzzyMatch(q, item.title);
      if (titleResult) {
        results.push({ item, result: titleResult, highlightInTitle: true });
        continue;
      }
      const haystack = `${item.title} ${item.subtitle ?? ""} ${item.keywords}`;
      const hayResult = fuzzyMatch(q, haystack);
      if (hayResult) {
        results.push({ item, result: { score: hayResult.score * 0.5, indices: [] }, highlightInTitle: false });
      }
    }
    results.sort((a, b) => b.result.score - a.result.score);
    return results;
  }, [items, query]);

  useEffect(() => {
    setSelected(0);
  }, [query]);

  useEffect(() => {
    const el = listRef.current?.querySelector(`[data-index="${selected}"]`);
    el?.scrollIntoView({ block: "nearest" });
  }, [selected]);

  if (!open) return null;

  const closeAfter = (action: PaletteAction) => {
    onClose();
    Promise.resolve(action.run()).catch((e) => toast.error(String(e)));
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Escape") {
      e.preventDefault();
      e.stopPropagation();
      e.nativeEvent.stopImmediatePropagation();
      onClose();
    } else if (e.key === "ArrowDown") {
      e.preventDefault();
      setSelected((s) => Math.min(s + 1, matched.length - 1));
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      setSelected((s) => Math.max(s - 1, 0));
    } else if (e.key === "Enter") {
      e.preventDefault();
      const m = matched[selected];
      if (m) closeAfter(m.item.primary);
    }
  };

  return (
    <div
      className="fixed inset-0 z-[100] flex items-start justify-center pt-[10vh] bg-black/60 backdrop-blur-sm"
      onMouseDown={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <div className="w-full max-w-xl mx-4 rounded-xl border border-border bg-background shadow-2xl overflow-hidden">
        <div className="flex items-center gap-2 px-3 py-2.5 border-b border-border">
          <Search className="h-4 w-4 text-muted-foreground shrink-0" />
          <Input
            ref={inputRef}
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Search projects and links..."
            className="h-8 border-none shadow-none focus-visible:ring-0 px-0"
          />
        </div>

        <div ref={listRef} className="max-h-[50vh] overflow-y-auto p-1">
          {matched.length === 0 ? (
            <p className="text-xs text-muted-foreground text-center py-8">
              Nothing found.
            </p>
          ) : (
            matched.map(({ item, result, highlightInTitle }, i) => (
              <div
                key={item.id}
                data-index={i}
                onMouseMove={() => setSelected(i)}
                onClick={() => closeAfter(item.primary)}
                className={cn(
                  "group flex items-center gap-3 rounded-lg px-2.5 py-2 cursor-pointer transition-colors",
                  i === selected ? "bg-accent" : "hover:bg-accent/50"
                )}
              >
                {item.icon}
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">
                    {highlightInTitle ? (
                      <HighlightedText text={item.title} indices={result.indices} />
                    ) : (
                      item.title
                    )}
                  </p>
                  {item.subtitle && (
                    <p className="text-xs text-muted-foreground truncate">{item.subtitle}</p>
                  )}
                </div>
                <div
                  className="flex items-center gap-0.5 shrink-0"
                  onClick={(e) => e.stopPropagation()}
                >
                  {item.actions.slice(0, 4).map((action) => (
                    <button
                      key={action.id}
                      title={action.label}
                      onClick={() => closeAfter(action)}
                      className={cn(
                        "flex items-center justify-center h-6 w-6 rounded-md transition-opacity hover:bg-accent",
                        action.id === item.primary.id
                          ? "opacity-80 hover:opacity-100"
                          : "opacity-50 hover:opacity-100"
                      )}
                    >
                      {action.icon}
                    </button>
                  ))}
                </div>
              </div>
            ))
          )}
        </div>

        <div className="flex items-center gap-4 px-3 py-1.5 border-t border-border text-[10px] text-muted-foreground">
          <span>
            <kbd className="px-1 py-0.5 bg-muted rounded">{getPaletteHotkeyLabel()}</kbd>{" "}
            toggle
          </span>
          <span>↑↓ navigate</span>
          <span>↵ select</span>
          <span>esc close</span>
        </div>
      </div>
    </div>
  );
}
