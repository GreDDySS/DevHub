import { describe, it, expect, vi, beforeEach } from "vitest";
import { useLinkStore } from "@/stores/linkStore";

vi.mock("@tauri-apps/api/core", () => ({
  invoke: vi.fn(),
}));

import { invoke } from "@tauri-apps/api/core";
const mockInvoke = vi.mocked(invoke);

const mockLinks = [
  {
    id: "1",
    url: "https://github.com/user/repo",
    title: "GitHub Repository",
    project_id: null,
    tags: [],
    notes: "",
    captured_at: "2026-01-01T00:00:00Z",
    created_at: "2026-01-01T00:00:00Z",
    updated_at: "2026-01-01T00:00:00Z",
  },
  {
    id: "2",
    url: "https://example.com/article",
    title: "Example Article",
    project_id: null,
    tags: [],
    notes: "",
    captured_at: "2026-01-02T00:00:00Z",
    created_at: "2026-01-02T00:00:00Z",
    updated_at: "2026-01-02T00:00:00Z",
  },
];

describe("linkStore", () => {
  beforeEach(() => {
    useLinkStore.setState({
      links: [],
      searchQuery: "",
      isLoading: false,
      error: null,
    });
    vi.clearAllMocks();
  });

  describe("fetchLinks", () => {
    it("calls invoke and sets links", async () => {
      mockInvoke.mockResolvedValue(mockLinks);
      await useLinkStore.getState().fetchLinks();
      expect(mockInvoke).toHaveBeenCalledWith("get_links");
      expect(useLinkStore.getState().links).toEqual(mockLinks);
      expect(useLinkStore.getState().isLoading).toBe(false);
    });

    it("sets error on failure", async () => {
      mockInvoke.mockRejectedValue("Network error");
      await useLinkStore.getState().fetchLinks();
      expect(useLinkStore.getState().error).toBe("Network error");
      expect(useLinkStore.getState().isLoading).toBe(false);
    });
  });

  describe("captureLink", () => {
    it("calls invoke and refetches", async () => {
      const newLink = { ...mockLinks[0], id: "3", url: "https://new.com" };
      mockInvoke.mockResolvedValueOnce(newLink);
      mockInvoke.mockResolvedValueOnce([...mockLinks, newLink]);

      const result = await useLinkStore.getState().captureLink("https://new.com");
      expect(result).toEqual(newLink);
      expect(mockInvoke).toHaveBeenCalledWith("capture_link", {
        url: "https://new.com",
      });
    });

    it("returns null and sets error on failure", async () => {
      mockInvoke.mockRejectedValue("Invalid URL");
      const result = await useLinkStore.getState().captureLink("bad");
      expect(result).toBeNull();
      expect(useLinkStore.getState().error).toBe("Invalid URL");
    });
  });

  describe("deleteLink", () => {
    it("calls invoke and refetches", async () => {
      mockInvoke.mockResolvedValueOnce(undefined);
      mockInvoke.mockResolvedValueOnce([]);
      const result = await useLinkStore.getState().deleteLink("1");
      expect(result).toBe(true);
      expect(mockInvoke).toHaveBeenCalledWith("delete_link", { id: "1" });
    });

    it("returns false on failure", async () => {
      mockInvoke.mockRejectedValue("Not found");
      const result = await useLinkStore.getState().deleteLink("999");
      expect(result).toBe(false);
    });
  });

  describe("setSearchQuery", () => {
    it("updates search query", () => {
      useLinkStore.getState().setSearchQuery("rust");
      expect(useLinkStore.getState().searchQuery).toBe("rust");
    });
  });
});
