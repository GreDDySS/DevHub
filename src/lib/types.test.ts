import { describe, it, expect } from "vitest";
import {
  LANGUAGE_ICONS,
  LANGUAGE_COLORS,
  STATUS_COLORS,
} from "@/lib/types";
import type {
  ProgrammingLanguage,
  ProjectStatus,
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

describe("LANGUAGE_COLORS", () => {
  it("has colors for all languages", () => {
    const languages: ProgrammingLanguage[] = [
      "CSharp", "Python", "Rust", "JavaScript", "TypeScript",
      "Go", "Java", "Cpp", "Other",
    ];
    for (const lang of languages) {
      expect(LANGUAGE_COLORS[lang]).toBeDefined();
      expect(LANGUAGE_COLORS[lang]).toContain("bg-");
      expect(LANGUAGE_COLORS[lang]).toContain("text-");
    }
  });
});

describe("STATUS_COLORS", () => {
  it("has colors for all statuses", () => {
    const statuses: ProjectStatus[] = ["Active", "Inactive"];
    for (const status of statuses) {
      expect(STATUS_COLORS[status]).toBeDefined();
      expect(STATUS_COLORS[status]).toContain("bg-");
      expect(STATUS_COLORS[status]).toContain("text-");
    }
  });
});
