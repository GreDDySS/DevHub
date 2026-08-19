import { describe, it, expect, vi, beforeEach } from "vitest";
import { useProjectStore } from "@/stores/projectStore";

vi.mock("@tauri-apps/api/core", () => ({
  invoke: vi.fn(),
}));

import { invoke } from "@tauri-apps/api/core";
const mockInvoke = vi.mocked(invoke);

const mockProjects = [
  {
    id: "1",
    name: "Test Project",
    path: "/test/path",
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
    name: "Another Project",
    path: "/another/path",
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

describe("projectStore", () => {
  beforeEach(() => {
    useProjectStore.setState({
      projects: [],
      filter: { show_hidden: false, sort_by: "name_asc" },
      isLoading: false,
      error: null,
      viewMode: "tiles",
    });
    vi.clearAllMocks();
  });

  describe("fetchProjects", () => {
    it("calls invoke and sets projects", async () => {
      mockInvoke.mockResolvedValue(mockProjects);
      await useProjectStore.getState().fetchProjects();
      expect(mockInvoke).toHaveBeenCalledWith("get_projects", {
        filter: expect.any(Object),
      });
      expect(useProjectStore.getState().projects).toEqual(mockProjects);
      expect(useProjectStore.getState().isLoading).toBe(false);
    });

    it("sets error on failure", async () => {
      mockInvoke.mockRejectedValue("Network error");
      await useProjectStore.getState().fetchProjects();
      expect(useProjectStore.getState().error).toBe("Network error");
      expect(useProjectStore.getState().isLoading).toBe(false);
    });

    it("sets isLoading during fetch", async () => {
      let resolvePromise: (value: unknown) => void;
      mockInvoke.mockReturnValue(
        new Promise((resolve) => {
          resolvePromise = resolve;
        })
      );

      const fetchPromise = useProjectStore.getState().fetchProjects();
      expect(useProjectStore.getState().isLoading).toBe(true);

      resolvePromise!([]);
      await fetchPromise;
      expect(useProjectStore.getState().isLoading).toBe(false);
    });
  });

  describe("addProject", () => {
    it("calls invoke and refetches projects", async () => {
      const newProject = { ...mockProjects[0], id: "3", name: "New" };
      mockInvoke.mockResolvedValueOnce(newProject);
      mockInvoke.mockResolvedValueOnce([...mockProjects, newProject]);

      const result = await useProjectStore.getState().addProject({
        name: "New",
        path: "/new/path",
      });

      expect(result).toEqual(newProject);
      expect(mockInvoke).toHaveBeenCalledWith("add_project", {
        request: { name: "New", path: "/new/path" },
      });
    });

    it("returns null and sets error on failure", async () => {
      mockInvoke.mockRejectedValue("Validation error");
      const result = await useProjectStore.getState().addProject({
        name: "Bad",
        path: "",
      });
      expect(result).toBeNull();
      expect(useProjectStore.getState().error).toBe("Validation error");
    });
  });

  describe("updateProject", () => {
    it("calls invoke and refetches", async () => {
      const updated = { ...mockProjects[0], name: "Updated" };
      mockInvoke.mockResolvedValueOnce(updated);
      mockInvoke.mockResolvedValueOnce([updated]);

      const result = await useProjectStore.getState().updateProject("1", {
        name: "Updated",
      });
      expect(result).toEqual(updated);
      expect(mockInvoke).toHaveBeenCalledWith("update_project", {
        id: "1",
        request: { name: "Updated" },
      });
    });
  });

  describe("deleteProject", () => {
    it("calls invoke and refetches", async () => {
      mockInvoke.mockResolvedValueOnce(undefined);
      mockInvoke.mockResolvedValueOnce([]);

      const result = await useProjectStore.getState().deleteProject("1");
      expect(result).toBe(true);
      expect(mockInvoke).toHaveBeenCalledWith("delete_project", { id: "1" });
    });

    it("returns false on failure", async () => {
      mockInvoke.mockRejectedValue("Not found");
      const result = await useProjectStore.getState().deleteProject("999");
      expect(result).toBe(false);
    });
  });

  describe("toggleFavorite", () => {
    it("calls invoke and refetches", async () => {
      mockInvoke.mockResolvedValueOnce(undefined);
      mockInvoke.mockResolvedValueOnce([]);
      await useProjectStore.getState().toggleFavorite("1");
      expect(mockInvoke).toHaveBeenCalledWith("toggle_favorite", { id: "1" });
    });

    it("sets error on failure", async () => {
      mockInvoke.mockRejectedValue("Not found");
      await useProjectStore.getState().toggleFavorite("999");
      expect(useProjectStore.getState().error).toBe("Not found");
    });
  });

  describe("toggleHidden", () => {
    it("calls invoke and refetches", async () => {
      mockInvoke.mockResolvedValueOnce(undefined);
      mockInvoke.mockResolvedValueOnce([]);
      await useProjectStore.getState().toggleHidden("1");
      expect(mockInvoke).toHaveBeenCalledWith("toggle_hidden", { id: "1" });
    });
  });

  describe("setFilter", () => {
    it("merges filter with existing state", () => {
      useProjectStore.getState().setFilter({ search_query: "test" });
      expect(useProjectStore.getState().filter.search_query).toBe("test");
      expect(useProjectStore.getState().filter.sort_by).toBe("name_asc");
    });

    it("replaces filter field", () => {
      useProjectStore.getState().setFilter({ status: "Active" });
      expect(useProjectStore.getState().filter.status).toBe("Active");
    });
  });

  describe("setViewMode", () => {
    it("toggles between tiles and list", () => {
      expect(useProjectStore.getState().viewMode).toBe("tiles");
      useProjectStore.getState().setViewMode("list");
      expect(useProjectStore.getState().viewMode).toBe("list");
      useProjectStore.getState().setViewMode("tiles");
      expect(useProjectStore.getState().viewMode).toBe("tiles");
    });
  });
});
