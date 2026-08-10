import { describe, it, expect } from "vitest";
import { cn, formatBytes, formatDate, truncate } from "@/lib/utils";

describe("cn", () => {
  it("merges class names", () => {
    const result = cn("text-red-500", "text-blue-500");
    expect(result).toBe("text-blue-500");
  });

  it("handles conditional classes", () => {
    const result = cn("base", false && "hidden", "extra");
    expect(result).toContain("base");
    expect(result).toContain("extra");
    expect(result).not.toContain("hidden");
  });

  it("returns empty string for no args", () => {
    expect(cn()).toBe("");
  });
});

describe("formatBytes", () => {
  it("formats 0 bytes", () => {
    expect(formatBytes(0)).toBe("0 Bytes");
  });

  it("formats bytes", () => {
    expect(formatBytes(500)).toBe("500 Bytes");
  });

  it("formats kilobytes", () => {
    expect(formatBytes(1024)).toBe("1 KB");
  });

  it("formats megabytes", () => {
    expect(formatBytes(1048576)).toBe("1 MB");
  });

  it("formats gigabytes", () => {
    expect(formatBytes(1073741824)).toBe("1 GB");
  });

  it("respects decimals parameter", () => {
    expect(formatBytes(1536, 1)).toBe("1.5 KB");
  });
});

describe("formatDate", () => {
  it("formats ISO date string", () => {
    const result = formatDate("2026-01-15T00:00:00Z");
    expect(result).toContain("Jan");
    expect(result).toContain("15");
    expect(result).toContain("2026");
  });

  it("formats Date object", () => {
    const date = new Date("2026-06-20T00:00:00Z");
    const result = formatDate(date);
    expect(result).toContain("Jun");
    expect(result).toContain("20");
  });
});

describe("truncate", () => {
  it("returns original string if shorter than limit", () => {
    expect(truncate("hello", 10)).toBe("hello");
  });

  it("truncates long strings", () => {
    expect(truncate("hello world", 5)).toBe("hello...");
  });

  it("returns exact string at limit length", () => {
    expect(truncate("hello", 5)).toBe("hello");
  });
});
