import { useEffect } from "react";
import { X, CheckCircle2, AlertCircle, Info } from "lucide-react";
import { useToastStore } from "@/stores/toastStore";
import { useProjectStore } from "@/stores/projectStore";
import { useLinkStore } from "@/stores/linkStore";
import { useTodoStore } from "@/stores/todoStore";
import { useSettingsStore } from "@/stores/settingsStore";
import { cn } from "@/lib/utils";

const ICONS = {
  success: <CheckCircle2 className="h-4 w-4 text-green-500 shrink-0" />,
  error: <AlertCircle className="h-4 w-4 text-destructive shrink-0" />,
  info: <Info className="h-4 w-4 text-blue-500 shrink-0" />,
};

const STORES_WITH_ERRORS = [
  useProjectStore,
  useLinkStore,
  useTodoStore,
  useSettingsStore,
] as const;

export function Toaster() {
  const toasts = useToastStore((s) => s.toasts);
  const dismiss = useToastStore((s) => s.dismiss);

  useEffect(() => {
    const unsubscribers = STORES_WITH_ERRORS.map((store) =>
      store.subscribe((state, prev) => {
        if (state.error && state.error !== prev.error) {
          useToastStore.getState().show("error", state.error);
        }
      })
    );
    return () => unsubscribers.forEach((unsub) => unsub());
  }, []);

  return (
    <div className="fixed bottom-4 right-4 z-[200] flex flex-col gap-2 max-w-sm">
      {toasts.map((t) => (
        <div
          key={t.id}
          className={cn(
            "flex items-center gap-2.5 rounded-lg border bg-background px-3 py-2.5 shadow-lg",
            t.type === "error" && "border-destructive/40"
          )}
        >
          {ICONS[t.type]}
          <p className="text-sm flex-1 min-w-0 break-words">{t.message}</p>
          <button
            onClick={() => dismiss(t.id)}
            className="text-muted-foreground hover:text-foreground transition-colors shrink-0"
          >
            <X className="h-3.5 w-3.5" />
          </button>
        </div>
      ))}
    </div>
  );
}
