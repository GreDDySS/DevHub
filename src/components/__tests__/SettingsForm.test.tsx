import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { SettingsForm } from "@/components/settings/SettingsForm";
import { useSettingsStore } from "@/stores/settingsStore";
import type { CloseAction } from "@/lib/types";

const { mockInvoke, mockSettings, mockSettingsEmptyIdes } = vi.hoisted(() => {
  const mockInvoke = vi.fn();
  const mockSettings = {
    version: 1,
    ides: [
      { name: "VS Code", path: "/usr/bin/code" },
      { name: "IntelliJ", path: "/opt/idea/bin/idea.sh" },
    ],
    default_ide_index: 0,
    autostart_enabled: false,
    close_action: "MinimizeToTray" as CloseAction,
    is_dark_theme: false,
    inactive_days: 30,
    statuses_enabled: true,
  };
  const mockSettingsEmptyIdes = {
    ...mockSettings,
    ides: [],
  };
  return { mockInvoke, mockSettings, mockSettingsEmptyIdes };
});

vi.mock("@tauri-apps/api/core", () => ({
  invoke: mockInvoke,
}));

vi.mock("@tauri-apps/plugin-dialog", () => ({
  open: vi.fn(),
}));

describe("SettingsForm", () => {
  beforeEach(() => {
    mockInvoke.mockResolvedValue(mockSettings);
    useSettingsStore.setState({
      settings: mockSettings,
      isLoading: false,
      isSaving: false,
      error: null,
    });
    vi.clearAllMocks();
    mockInvoke.mockResolvedValue(mockSettings);
  });

  it("renders the Settings heading", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Settings")).toBeInTheDocument();
  });

  it("shows loading state initially", () => {
    useSettingsStore.setState({ isLoading: true, settings: undefined as never });
    render(<SettingsForm />);
    expect(screen.getByText("Loading settings...")).toBeInTheDocument();
  });

  it("renders save confirmation badge", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Settings")).toBeInTheDocument();
  });

  it("renders appearance section with theme switch", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Appearance")).toBeInTheDocument();
    expect(screen.getByText("Dark Theme")).toBeInTheDocument();
  });

  it("renders behavior section with close action", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Behavior")).toBeInTheDocument();
    expect(screen.getByText("Close Action")).toBeInTheDocument();
    expect(screen.getByText("Minimize to Tray")).toBeInTheDocument();
    expect(screen.getByText("Exit App")).toBeInTheDocument();
    expect(screen.getByText("Ask Me")).toBeInTheDocument();
  });

  it("renders autostart toggle in behavior section", async () => {
    render(<SettingsForm />);
    await screen.findByText("Behavior");
    expect(screen.getByText("Autostart")).toBeInTheDocument();
  });

  it("renders idle days input in behavior section", async () => {
    render(<SettingsForm />);
    await screen.findByText("Behavior");
    expect(screen.getByText("Inactive After (days)")).toBeInTheDocument();
  });

  it("renders IDEs section", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("IDEs")).toBeInTheDocument();
    expect(
      screen.getByText("Configure IDEs for launching projects")
    ).toBeInTheDocument();
  });

  it("renders add IDE button", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Add IDE")).toBeInTheDocument();
  });

  it("renders scan IDEs button", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Scan for IDEs")).toBeInTheDocument();
  });

  it("renders keyboard shortcuts section", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Keyboard Shortcuts")).toBeInTheDocument();
    expect(screen.getByText("Ctrl + Shift + Y")).toBeInTheDocument();
  });

  it("renders data management section", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Data Management")).toBeInTheDocument();
    expect(screen.getByText("Restore Defaults")).toBeInTheDocument();
  });

  it("renders IDE entries when configured", async () => {
    render(<SettingsForm />);
    expect(await screen.findByDisplayValue("/usr/bin/code")).toBeInTheDocument();
    expect(screen.getByDisplayValue("/opt/idea/bin/idea.sh")).toBeInTheDocument();
  });

  it("renders empty state when no IDEs configured", async () => {
    mockInvoke.mockResolvedValue(mockSettingsEmptyIdes);
    render(<SettingsForm />);
    expect(await screen.findByText("No IDEs configured yet")).toBeInTheDocument();
  });
});
