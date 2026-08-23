import { describe, it, expect } from "vitest";
import { fuzzyMatch } from "./fuzzy";

describe("fuzzyMatch", () => {
  it("matches subsequence case-insensitively", () => {
    const result = fuzzyMatch("vsc", "Visual Studio Code");
    expect(result).not.toBeNull();
    expect(result!.indices).toEqual([0, 2, 14]);
  });

  it("returns null when characters are out of order", () => {
    expect(fuzzyMatch("cs", "vs code")).toBeNull();
  });

  it("returns null when a character is missing", () => {
    expect(fuzzyMatch("xyz", "visual studio code")).toBeNull();
  });

  it("returns empty indices for empty query", () => {
    const result = fuzzyMatch("", "anything");
    expect(result).toEqual({ score: 0, indices: [] });
  });

  it("returns null when query is longer than text", () => {
    expect(fuzzyMatch("verylongquery", "abc")).toBeNull();
  });

  it("scores consecutive matches higher than scattered ones", () => {
    const consecutive = fuzzyMatch("cod", "code");
    const scattered = fuzzyMatch("cod", "c o d e");
    expect(consecutive!.score).toBeGreaterThan(scattered!.score);
  });

  it("scores word-boundary matches higher than mid-word ones", () => {
    const boundary = fuzzyMatch("s", "my-script");
    const middle = fuzzyMatch("s", "ascript");
    expect(boundary!.score).toBeGreaterThan(middle!.score);
  });

  it("prefers shorter texts on equal matches", () => {
    const short = fuzzyMatch("app", "app");
    const long = fuzzyMatch("app", "application folder");
    expect(short!.score).toBeGreaterThan(long!.score);
  });

  it("handles special path separators as boundaries", () => {
    const result = fuzzyMatch("sr", "src/main.rs");
    expect(result).not.toBeNull();
    expect(result!.indices).toEqual([0, 1]);
  });
});
