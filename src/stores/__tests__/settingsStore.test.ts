import { describe, it, expect, vi, beforeEach } from "vitest";
import { useSettingsStore, defaultSettings } from "@/stores/settingsStore";

vi.mock("@tauri-apps/api/core", () => ({
  invoke: vi.fn(),
}));

import { invoke } from "@tauri-apps/api/core";
const mockInvoke = vi.mocked(invoke);

const mockSettings = {
  version: 1,
  ides: [{ name: "VS Code", path: "C:\\Code.exe" }],
  default_ide_index: 0,
  autostart_enabled: false,
  close_action: "MinimizeToTray" as const,
  is_dark_theme: false,
  inactive_days: 30,
  statuses_enabled: true,
};

describe("settingsStore", () => {
  beforeEach(() => {
    useSettingsStore.setState({
      settings: {
        version: 1,
        ides: [],
        default_ide_index: 0,
        autostart_enabled: false,
        close_action: "MinimizeToTray",
        is_dark_theme: false,
        inactive_days: 30,
        statuses_enabled: true,
      },
      isLoading: false,
      isSaving: false,
      error: null,
    });
    vi.clearAllMocks();
  });

  describe("fetchSettings", () => {
    it("calls invoke and sets settings", async () => {
      mockInvoke.mockResolvedValue(mockSettings);
      await useSettingsStore.getState().fetchSettings();
      expect(mockInvoke).toHaveBeenCalledWith("get_settings");
      expect(useSettingsStore.getState().settings).toEqual(mockSettings);
      expect(useSettingsStore.getState().isLoading).toBe(false);
    });

    it("sets error and falls back to defaults on failure", async () => {
      mockInvoke.mockRejectedValue("IPC error");
      await useSettingsStore.getState().fetchSettings();
      expect(useSettingsStore.getState().error).toBe("IPC error");
      expect(useSettingsStore.getState().settings.ides).toEqual([]);
    });
  });

  describe("saveSettings", () => {
    it("calls invoke and updates state", async () => {
      mockInvoke.mockResolvedValue(undefined);
      await useSettingsStore.getState().saveSettings(mockSettings);
      expect(mockInvoke).toHaveBeenCalledWith("save_settings", {
        settings: mockSettings,
      });
      expect(useSettingsStore.getState().settings).toEqual(mockSettings);
    });

    it("sets error on failure", async () => {
      mockInvoke.mockRejectedValue("Write failed");
      await useSettingsStore.getState().saveSettings(mockSettings);
      expect(useSettingsStore.getState().error).toBe("Write failed");
    });
  });

  describe("updateSettings", () => {
    it("merges patch into current settings and saves", async () => {
      mockInvoke.mockResolvedValue(undefined);
      const current = useSettingsStore.getState().settings;
      await useSettingsStore.getState().updateSettings({
        is_dark_theme: true,
        autostart_enabled: true,
      });
      expect(mockInvoke).toHaveBeenCalledWith("save_settings", {
        settings: expect.objectContaining({
          is_dark_theme: true,
          autostart_enabled: true,
          inactive_days: current.inactive_days,
        }),
      });
      expect(useSettingsStore.getState().settings.is_dark_theme).toBe(true);
      expect(useSettingsStore.getState().settings.autostart_enabled).toBe(true);
    });
  });

  describe("restoreDefaults", () => {
    it("saves default settings", async () => {
      mockInvoke.mockResolvedValue(undefined);
      await useSettingsStore.getState().restoreDefaults();
      expect(mockInvoke).toHaveBeenCalledWith("save_settings", {
        settings: defaultSettings,
      });
      expect(useSettingsStore.getState().settings).toEqual(defaultSettings);
    });
  });

  describe("toggleTheme", () => {
    it("toggles is_dark_theme and saves", async () => {
      mockInvoke.mockResolvedValue(undefined);
      expect(useSettingsStore.getState().settings.is_dark_theme).toBe(false);

      await useSettingsStore.getState().toggleTheme();
      expect(useSettingsStore.getState().settings.is_dark_theme).toBe(true);
      expect(mockInvoke).toHaveBeenCalled();
    });

    it("toggles back to light", async () => {
      useSettingsStore.setState({
        settings: { ...useSettingsStore.getState().settings, is_dark_theme: true },
      });
      mockInvoke.mockResolvedValue(undefined);

      await useSettingsStore.getState().toggleTheme();
      expect(useSettingsStore.getState().settings.is_dark_theme).toBe(false);
    });
  });
});
