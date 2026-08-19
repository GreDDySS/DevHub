import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { ProjectList } from "@/components/projects/ProjectList";
import { useProjectStore } from "@/stores/projectStore";

const mockProjects = [
  {
    id: "1",
    name: "My Rust App",
    path: "/test",
    description: "",
    language: "Rust" as const,
    status: "Active" as const,
    tags: [],
    preferred_ide: null,
    is_favorite: false,
    is_hidden: false,
    created_at: "2026-01-01T00:00:00Z",
    updated_at: "2026-01-01T00:00:00Z",
  },
  {
    id: "2",
    name: "Web Project",
    path: "/web",
    description: "",
    language: "TypeScript" as const,
    status: "Active" as const,
    tags: [],
    preferred_ide: null,
    is_favorite: true,
    is_hidden: false,
    created_at: "2026-01-02T00:00:00Z",
    updated_at: "2026-01-02T00:00:00Z",
  },
];

const mockInvoke = vi.fn().mockResolvedValue(mockProjects);

vi.mock("@tauri-apps/api/core", () => ({
  invoke: (...args: unknown[]) => mockInvoke(...args),
}));

describe("ProjectList", () => {
  beforeEach(() => {
    mockInvoke.mockResolvedValue(mockProjects);
    useProjectStore.setState({
      projects: [],
      filter: { show_hidden: false, sort_by: "name_asc" },
      isLoading: false,
      error: null,
      viewMode: "tiles",
    });
  });

  it("renders the Projects heading", async () => {
    render(<ProjectList />);
    expect(screen.getByText("Projects")).toBeInTheDocument();
  });

  it("shows loading state initially", () => {
    useProjectStore.setState({ isLoading: true });
    render(<ProjectList />);
    expect(screen.getByText("Loading...")).toBeInTheDocument();
  });

  it("shows empty state when no projects", async () => {
    mockInvoke.mockResolvedValue([]);
    render(<ProjectList />);
    expect(await screen.findByText("No projects found")).toBeInTheDocument();
  });

  it("renders project cards when projects exist", async () => {
    render(<ProjectList />);
    expect(await screen.findByText("My Rust App")).toBeInTheDocument();
    expect(screen.getByText("Web Project")).toBeInTheDocument();
  });

  it("renders search input", () => {
    render(<ProjectList />);
    expect(screen.getByPlaceholderText("Search projects...")).toBeInTheDocument();
  });

  it("renders add project button", () => {
    render(<ProjectList />);
    expect(screen.getByTitle("Add project")).toBeInTheDocument();
  });

  it("renders scan button", () => {
    render(<ProjectList />);
    expect(screen.getByTitle("Scan for projects")).toBeInTheDocument();
  });

  it("renders view mode toggle", () => {
    render(<ProjectList />);
    expect(screen.getByTitle("List view")).toBeInTheDocument();
  });

  it("does not show hidden projects when filter excludes them", async () => {
    const visibleProject = { ...mockProjects[0], name: "Visible" };
    const hiddenProject = {
      ...mockProjects[1],
      id: "3",
      name: "Hidden",
      is_hidden: true,
    };
    mockInvoke.mockResolvedValue([visibleProject]);
    render(<ProjectList />);
    expect(await screen.findByText("Visible")).toBeInTheDocument();
    expect(screen.queryByText("Hidden")).not.toBeInTheDocument();
  });
});
