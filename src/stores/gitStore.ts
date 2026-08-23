import { create } from "zustand";
import { invoke } from "@tauri-apps/api/core";
import type { GitActivity } from "@/lib/types";

interface GitState {
  activity: GitActivity | null;
  isLoading: boolean;
  error: string | null;

  fetchGitActivity: (projectPath: string) => Promise<void>;
  clearGitActivity: () => void;
}

export const useGitStore = create<GitState>((set) => ({
  activity: null,
  isLoading: false,
  error: null,

  fetchGitActivity: async (projectPath) => {
    set({ isLoading: true, error: null });
    try {
      const activity = await invoke<GitActivity | null>("get_git_activity", {
        projectPath,
      });
      set({ activity, isLoading: false });
    } catch (error) {
      set({
        error: String(error),
        isLoading: false,
        activity: null,
      });
    }
  },

  clearGitActivity: () => set({ activity: null, error: null }),
}));
