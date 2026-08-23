import { create } from "zustand";

interface UiState {
  activeView: string;
  setActiveView: (view: string) => void;
  detailProjectId: string | null;
  openProjectDetail: (projectId: string) => void;
  closeProjectDetail: () => void;
}

export const useUiStore = create<UiState>((set) => ({
  activeView: "projects",
  setActiveView: (view) => set({ activeView: view }),
  detailProjectId: null,
  openProjectDetail: (projectId) =>
    set({ detailProjectId: projectId, activeView: "projects" }),
  closeProjectDetail: () => set({ detailProjectId: null }),
}));
