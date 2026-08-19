import { create } from "zustand";
import { invoke } from "@tauri-apps/api/core";
import type {
  Project,
  ProjectFilter,
  CreateProjectRequest,
  UpdateProjectRequest,
} from "@/lib/types";

interface ProjectState {
  projects: Project[];
  filter: ProjectFilter;
  isLoading: boolean;
  error: string | null;
  viewMode: "tiles" | "list";

  setFilter: (filter: Partial<ProjectFilter>) => void;
  setViewMode: (mode: "tiles" | "list") => void;
  fetchProjects: () => Promise<void>;
  refreshProjects: () => Promise<void>;
  addProject: (request: CreateProjectRequest) => Promise<Project | null>;
  updateProject: (
    id: string,
    request: UpdateProjectRequest
  ) => Promise<Project | null>;
  deleteProject: (id: string) => Promise<boolean>;
  toggleFavorite: (id: string) => Promise<void>;
  toggleHidden: (id: string) => Promise<void>;
  openInExplorer: (path: string) => Promise<void>;
  openInIde: (projectPath: string, idePath: string) => Promise<void>;
  openInConsole: (path: string) => Promise<void>;
}

export const useProjectStore = create<ProjectState>((set, get) => ({
  projects: [],
  filter: { show_hidden: false, sort_by: "name_asc" },
  isLoading: false,
  error: null,
  viewMode: "tiles",

  setFilter: (filter) =>
    set((state) => ({ filter: { ...state.filter, ...filter } })),

  setViewMode: (mode) => set({ viewMode: mode }),

  fetchProjects: async () => {
    set({ isLoading: true, error: null });
    try {
      const filter = get().filter;
      const projects = await invoke<Project[]>("get_projects", { filter });
      set({ projects, isLoading: false });
    } catch (error) {
      set({ error: String(error), isLoading: false });
    }
  },

  refreshProjects: async () => {
    set({ isLoading: true, error: null });
    try {
      const projects = await invoke<Project[]>("refresh_projects");
      const filter = get().filter;
      const filtered = await invoke<Project[]>("get_projects", { filter });
      set({ projects: filtered, isLoading: false });
    } catch (error) {
      set({ error: String(error), isLoading: false });
    }
  },

  addProject: async (request) => {
    try {
      const project = await invoke<Project>("add_project", { request });
      await get().fetchProjects();
      return project;
    } catch (error) {
      set({ error: String(error) });
      return null;
    }
  },

  updateProject: async (id, request) => {
    try {
      const project = await invoke<Project>("update_project", {
        id,
        request,
      });
      await get().fetchProjects();
      return project;
    } catch (error) {
      set({ error: String(error) });
      return null;
    }
  },

  deleteProject: async (id) => {
    try {
      await invoke("delete_project", { id });
      await get().fetchProjects();
      return true;
    } catch (error) {
      set({ error: String(error) });
      return false;
    }
  },

  toggleFavorite: async (id) => {
    try {
      await invoke("toggle_favorite", { id });
      await get().fetchProjects();
    } catch (error) {
      set({ error: String(error) });
    }
  },

  toggleHidden: async (id) => {
    try {
      await invoke("toggle_hidden", { id });
      await get().fetchProjects();
    } catch (error) {
      set({ error: String(error) });
    }
  },

  openInExplorer: async (path) => {
    try {
      await invoke("open_in_explorer", { path });
    } catch (error) {
      set({ error: String(error) });
    }
  },

  openInIde: async (projectPath, idePath) => {
    try {
      await invoke("open_in_ide", { projectPath, idePath });
    } catch (error) {
      set({ error: String(error) });
    }
  },

  openInConsole: async (path) => {
    try {
      await invoke("open_in_console", { path });
    } catch (error) {
      set({ error: String(error) });
    }
  },
}));
