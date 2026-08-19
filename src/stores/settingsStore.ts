import { create } from "zustand";
import { invoke } from "@tauri-apps/api/core";
import type { AppSettings } from "@/lib/types";

export const defaultSettings: AppSettings = {
  version: 1,
  ides: [],
  default_ide_index: 0,
  autostart_enabled: false,
  close_action: "MinimizeToTray",
  is_dark_theme: false,
  inactive_days: 30,
  statuses_enabled: true,
};

interface SettingsState {
  settings: AppSettings;
  isLoading: boolean;
  error: string | null;
  isSaving: boolean;

  fetchSettings: () => Promise<void>;
  saveSettings: (settings: AppSettings) => Promise<void>;
  updateSettings: (patch: Partial<AppSettings>) => Promise<void>;
  restoreDefaults: () => Promise<void>;
  toggleTheme: () => Promise<void>;
}

export const useSettingsStore = create<SettingsState>((set, get) => ({
  settings: defaultSettings,
  isLoading: false,
  isSaving: false,
  error: null,

  fetchSettings: async () => {
    set({ isLoading: true, error: null });
    try {
      const settings = await invoke<AppSettings>("get_settings");
      set({ settings, isLoading: false });

      applyTheme(settings.is_dark_theme);
    } catch (error) {
      set({ settings: defaultSettings, isLoading: false, error: String(error) });
    }
  },

  saveSettings: async (settings) => {
    set({ isSaving: true, error: null });
    try {
      await invoke("save_settings", { settings });
      set({ settings, isSaving: false });
      applyTheme(settings.is_dark_theme);
    } catch (error) {
      set({ error: String(error), isSaving: false });
    }
  },

  updateSettings: async (patch) => {
    const current = get().settings;
    const updated = { ...current, ...patch };
    await get().saveSettings(updated);
  },

  restoreDefaults: async () => {
    await get().saveSettings({ ...defaultSettings });
  },

  toggleTheme: async () => {
    const settings = get().settings;
    await get().updateSettings({ is_dark_theme: !settings.is_dark_theme });
  },
}));

function applyTheme(isDark: boolean) {
  if (isDark) {
    document.documentElement.classList.add("dark");
  } else {
    document.documentElement.classList.remove("dark");
  }
}
