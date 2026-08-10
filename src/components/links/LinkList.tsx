import React, { useState, useEffect } from "react";
import {
  Search,
  Plus,
  ExternalLink,
  Copy,
  Trash2,
  FolderOpen,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { CaptureLinkDialog } from "./CaptureLinkDialog";
import { useLinkStore } from "@/stores/linkStore";
import type { Link, LinkType } from "@/lib/types";
import { LINK_TYPE_COLORS } from "@/lib/types";
import { cn } from "@/lib/utils";

const typeFilters: { label: string; value: LinkType | null }[] = [
  { label: "All", value: null },
  { label: "YouTube", value: "YouTube" },
  { label: "Article", value: "Article" },
  { label: "Repository", value: "Repository" },
  { label: "Docs", value: "Documentation" },
  { label: "Other", value: "Other" },
];

export function LinkList() {
  const {
    links,
    searchQuery,
    typeFilter,
    isLoading,
    setSearchQuery,
    setTypeFilter,
    fetchLinks,
    deleteLink,
    copyUrl,
    openInBrowser,
  } = useLinkStore();

  const [showCaptureDialog, setShowCaptureDialog] = useState(false);

  useEffect(() => {
    fetchLinks();
  }, []);

  const filteredLinks = links.filter((link) => {
    const matchesSearch =
      !searchQuery ||
      link.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      link.url.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesType = !typeFilter || link.type === typeFilter;
    return matchesSearch && matchesType;
  });

  const handleCopyUrl = (e: React.MouseEvent, url: string) => {
    e.stopPropagation();
    copyUrl(url);
  };

  const handleOpenInBrowser = (e: React.MouseEvent, url: string) => {
    e.stopPropagation();
    openInBrowser(url);
  };

  const handleDelete = (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    deleteLink(id);
  };

  if (isLoading) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground">
        Loading...
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Links</h1>
        <Button size="sm" onClick={() => setShowCaptureDialog(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Capture Link
        </Button>
      </div>

      <div className="flex items-center gap-4 mb-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search links..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9"
          />
        </div>
        <div className="flex items-center gap-1">
          {typeFilters.map((tf) => (
            <Button
              key={tf.label}
              variant={typeFilter === tf.value ? "default" : "ghost"}
              size="sm"
              onClick={() => setTypeFilter(tf.value)}
            >
              {tf.label}
            </Button>
          ))}
        </div>
      </div>

      {filteredLinks.length === 0 ? (
        <div className="flex-1 flex flex-col items-center justify-center text-muted-foreground">
          <FolderOpen className="h-12 w-12 mb-4" />
          <p className="text-lg">No links found</p>
          <p className="text-sm">Capture a link to get started</p>
        </div>
      ) : (
        <div className="flex flex-col gap-2">
          {filteredLinks.map((link) => (
            <LinkItem
              key={link.id}
              link={link}
              onCopyUrl={handleCopyUrl}
              onOpenInBrowser={handleOpenInBrowser}
              onDelete={handleDelete}
            />
          ))}
        </div>
      )}

      {showCaptureDialog && (
        <CaptureLinkDialog onClose={() => setShowCaptureDialog(false)} />
      )}
    </div>
  );
}

interface LinkItemProps {
  link: Link;
  onCopyUrl: (e: React.MouseEvent, url: string) => void;
  onOpenInBrowser: (e: React.MouseEvent, url: string) => void;
  onDelete: (e: React.MouseEvent, id: string) => void;
}

function LinkItem({
  link,
  onCopyUrl,
  onOpenInBrowser,
  onDelete,
}: LinkItemProps) {
  return (
    <div className="flex items-center gap-4 p-3 rounded-lg border bg-card hover:bg-accent/50 cursor-pointer transition-colors">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <h3 className="font-medium truncate">{link.title || link.url}</h3>
          <Badge className={cn("text-xs", LINK_TYPE_COLORS[link.type])}>
            {link.type}
          </Badge>
        </div>
        <p className="text-sm text-muted-foreground truncate">{link.url}</p>
      </div>
      <div className="flex items-center gap-1">
        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8"
          onClick={(e) => onCopyUrl(e, link.url)}
        >
          <Copy className="h-4 w-4 text-muted-foreground" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8"
          onClick={(e) => onOpenInBrowser(e, link.url)}
        >
          <ExternalLink className="h-4 w-4 text-muted-foreground" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8"
          onClick={(e) => onDelete(e, link.id)}
        >
          <Trash2 className="h-4 w-4 text-muted-foreground" />
        </Button>
      </div>
    </div>
  );
}
