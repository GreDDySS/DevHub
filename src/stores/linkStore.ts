import { create } from "zustand";
import { invoke } from "@tauri-apps/api/core";
import type { Link } from "@/lib/types";
import { toast } from "@/stores/toastStore";

interface LinkState {
  links: Link[];
  searchQuery: string;
  isLoading: boolean;
  error: string | null;

  setSearchQuery: (query: string) => void;
  fetchLinks: () => Promise<void>;
  captureLink: (url: string) => Promise<Link | null>;
  addLink: (
    url: string,
    options?: { title?: string; projectId?: string }
  ) => Promise<Link | null>;
  addLinkFromClipboard: () => Promise<Link | null>;
  deleteLink: (id: string) => Promise<boolean>;
  copyUrl: (url: string) => Promise<void>;
  openInBrowser: (url: string) => Promise<void>;
}

export const useLinkStore = create<LinkState>((set, get) => ({
  links: [],
  searchQuery: "",
  isLoading: false,
  error: null,

  setSearchQuery: (query) => set({ searchQuery: query }),

  fetchLinks: async () => {
    set({ isLoading: true, error: null });
    try {
      const links = await invoke<Link[]>("get_links");
      set({ links, isLoading: false });
    } catch (error) {
      set({ error: String(error), isLoading: false });
    }
  },

  captureLink: async (url: string) => {
    try {
      const link = await invoke<Link>("capture_link", { url });
      await get().fetchLinks();
      return link;
    } catch (error) {
      set({ error: String(error) });
      return null;
    }
  },

  addLink: async (
    url: string,
    options?: { title?: string; projectId?: string }
  ) => {
    try {
      const link = await invoke<Link>("add_link", {
        url,
        title: options?.title ?? null,
        projectId: options?.projectId ?? null,
      });
      await get().fetchLinks();
      return link;
    } catch (error) {
      set({ error: String(error) });
      return null;
    }
  },

  addLinkFromClipboard: async () => {
    try {
      const { readText } = await import("@tauri-apps/plugin-clipboard-manager");
      const text = await readText();
      if (!text || !text.startsWith("http")) {
        set({ error: "No valid URL in clipboard" });
        return null;
      }
      const link = await invoke<Link>("capture_link", { url: text });
      await get().fetchLinks();
      return link;
    } catch (error) {
      set({ error: String(error) });
      return null;
    }
  },

  deleteLink: async (id) => {
    try {
      await invoke("delete_link", { id });
      await get().fetchLinks();
      return true;
    } catch (error) {
      set({ error: String(error) });
      return false;
    }
  },

  copyUrl: async (url) => {
    try {
      const { writeText } = await import("@tauri-apps/plugin-clipboard-manager");
      await writeText(url);
      toast.success("URL copied to clipboard");
    } catch (error) {
      set({ error: String(error) });
    }
  },

  openInBrowser: async (url) => {
    try {
      await invoke("open_in_browser", { url });
    } catch (error) {
      set({ error: String(error) });
    }
  },
}));
