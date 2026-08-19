import { describe, it, expect, vi, beforeEach, waitFor } from "vitest";
import { render, screen } from "@testing-library/react";
import { SettingsForm } from "@/components/settings/SettingsForm";
import { useSettingsStore } from "@/stores/settingsStore";

vi.mock("@tauri-apps/api/core", () => ({
  invoke: vi.fn().mockResolvedValue({
    version: 1,
    ides: [],
    default_ide_index: 0,
    autostart_enabled: false,
    close_action: "MinimizeToTray",
    is_dark_theme: false,
  }),
}));

describe("SettingsForm", () => {
  beforeEach(() => {
    useSettingsStore.setState({
      settings: {
        version: 1,
        ides: [],
        default_ide_index: 0,
        autostart_enabled: false,
        close_action: "MinimizeToTray",
        is_dark_theme: false,
      },
      isLoading: false,
      error: null,
    });
  });

  it("renders the Settings heading", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Settings")).toBeInTheDocument();
  });

  it("shows loading state initially", () => {
    useSettingsStore.setState({ isLoading: true, settings: undefined as never });
    render(<SettingsForm />);
    expect(screen.getByText("Loading...")).toBeInTheDocument();
  });

  it("renders save button", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Save")).toBeInTheDocument();
  });

  it("renders IDE section", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("IDEs")).toBeInTheDocument();
    expect(screen.getByText("Configure your preferred IDEs")).toBeInTheDocument();
  });

  it("renders close action section", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Close Action")).toBeInTheDocument();
  });

  it("renders close action buttons", async () => {
    render(<SettingsForm />);
    await screen.findByText("Exit");
    expect(screen.getByText("Exit")).toBeInTheDocument();
    expect(screen.getByText("Minimize to Tray")).toBeInTheDocument();
    expect(screen.getByText("Ask")).toBeInTheDocument();
  });

  it("renders autostart section", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Autostart")).toBeInTheDocument();
  });

  it("renders add IDE button", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Add IDE")).toBeInTheDocument();
  });

  it("renders scan IDEs button", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Scan for IDEs")).toBeInTheDocument();
  });

  it("shows disabled autostart by default", async () => {
    render(<SettingsForm />);
    expect(await screen.findByText("Disabled")).toBeInTheDocument();
  });
});
