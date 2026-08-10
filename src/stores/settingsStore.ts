import { create } from "zustand";
import { invoke } from "@tauri-apps/api/core";
import type { AppSettings } from "@/lib/types";

interface SettingsState {
  settings: AppSettings | null;
  isLoading: boolean;
  error: string | null;

  fetchSettings: () => Promise<void>;
  saveSettings: (settings: AppSettings) => Promise<void>;
  toggleTheme: () => Promise<void>;
}

const defaultSettings: AppSettings = {
  ides: [],
  default_ide_index: 0,
  autostart_enabled: false,
  close_action: "MinimizeToTray",
  is_dark_theme: false,
};

export const useSettingsStore = create<SettingsState>((set, get) => ({
  settings: null,
  isLoading: false,
  error: null,

  fetchSettings: async () => {
    set({ isLoading: true, error: null });
    try {
      const settings = await invoke<AppSettings>("get_settings");
      set({ settings, isLoading: false });

      if (settings.is_dark_theme) {
        document.documentElement.classList.add("dark");
      } else {
        document.documentElement.classList.remove("dark");
      }
    } catch (error) {
      set({ settings: defaultSettings, isLoading: false, error: String(error) });
    }
  },

  saveSettings: async (settings) => {
    try {
      await invoke("save_settings", { settings });
      set({ settings });

      if (settings.is_dark_theme) {
        document.documentElement.classList.add("dark");
      } else {
        document.documentElement.classList.remove("dark");
      }
    } catch (error) {
      set({ error: String(error) });
    }
  },

  toggleTheme: async () => {
    const settings = get().settings;
    if (!settings) return;

    const newSettings = { ...settings, is_dark_theme: !settings.is_dark_theme };
    await get().saveSettings(newSettings);
  },
}));
