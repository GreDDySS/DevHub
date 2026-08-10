import { describe, it, expect } from "vitest";
import {
  LANGUAGE_ICONS,
  STATUS_COLORS,
  LINK_TYPE_COLORS,
} from "@/lib/types";
import type {
  ProgrammingLanguage,
  ProjectStatus,
  LinkType,
} from "@/lib/types";

describe("LANGUAGE_ICONS", () => {
  it("has icons for all languages", () => {
    const languages: ProgrammingLanguage[] = [
      "CSharp", "Python", "Rust", "JavaScript", "TypeScript",
      "Go", "Java", "Cpp", "Other",
    ];
    for (const lang of languages) {
      expect(LANGUAGE_ICONS[lang]).toBeDefined();
      expect(typeof LANGUAGE_ICONS[lang]).toBe("string");
    }
  });

  it("has correct C# icon", () => {
    expect(LANGUAGE_ICONS.CSharp).toBe("C#");
  });
});

describe("STATUS_COLORS", () => {
  it("has colors for all statuses", () => {
    const statuses: ProjectStatus[] = ["Active", "Completed", "Paused", "Archived"];
    for (const status of statuses) {
      expect(STATUS_COLORS[status]).toBeDefined();
      expect(STATUS_COLORS[status]).toContain("bg-");
      expect(STATUS_COLORS[status]).toContain("text-");
    }
  });
});

describe("LINK_TYPE_COLORS", () => {
  it("has colors for all link types", () => {
    const types: LinkType[] = ["YouTube", "Article", "Repository", "Documentation", "Other"];
    for (const type of types) {
      expect(LINK_TYPE_COLORS[type]).toBeDefined();
      expect(LINK_TYPE_COLORS[type]).toContain("bg-");
    }
  });
});
