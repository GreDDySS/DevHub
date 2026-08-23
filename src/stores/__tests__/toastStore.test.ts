import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { useToastStore, toast } from "../toastStore";

describe("toastStore", () => {
  beforeEach(() => {
    useToastStore.setState({ toasts: [] });
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("adds a toast with type and message", () => {
    toast.success("Done");
    const { toasts } = useToastStore.getState();
    expect(toasts).toHaveLength(1);
    expect(toasts[0].type).toBe("success");
    expect(toasts[0].message).toBe("Done");
  });

  it("auto-dismisses after timeout", () => {
    toast.info("Temporary");
    expect(useToastStore.getState().toasts).toHaveLength(1);
    vi.advanceTimersByTime(4000);
    expect(useToastStore.getState().toasts).toHaveLength(0);
  });

  it("keeps at most 5 toasts", () => {
    for (let i = 0; i < 8; i++) {
      toast.error(`Error ${i}`);
    }
    const { toasts } = useToastStore.getState();
    expect(toasts).toHaveLength(5);
    expect(toasts[4].message).toBe("Error 7");
  });

  it("dismiss removes a specific toast", () => {
    toast.success("First");
    toast.success("Second");
    const [first] = useToastStore.getState().toasts;
    useToastStore.getState().dismiss(first.id);
    const { toasts } = useToastStore.getState();
    expect(toasts).toHaveLength(1);
    expect(toasts[0].message).toBe("Second");
  });
});
