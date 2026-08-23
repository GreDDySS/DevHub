import { create } from "zustand";
import { invoke } from "@tauri-apps/api/core";
import type { ProjectStats } from "@/lib/types";

interface StatsState {
  stats: ProjectStats | null;
  isLoading: boolean;
  error: string | null;

  fetchStats: (projectPath: string) => Promise<void>;
  clearStats: () => void;
}

export const useStatsStore = create<StatsState>((set) => ({
  stats: null,
  isLoading: false,
  error: null,

  fetchStats: async (projectPath) => {
    set({ isLoading: true, error: null });
    try {
      const stats = await invoke<ProjectStats | null>("get_project_stats", {
        projectPath,
      });
      set({ stats, isLoading: false });
    } catch (error) {
      set({ error: String(error), isLoading: false, stats: null });
    }
  },

  clearStats: () => set({ stats: null, error: null }),
}));
