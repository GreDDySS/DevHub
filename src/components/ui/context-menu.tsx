import * as React from "react";
import { cn } from "@/lib/utils";

interface ContextMenuContextValue {
  open: boolean;
  setOpen: (open: boolean) => void;
  pos: { x: number; y: number };
  setPos: (pos: { x: number; y: number }) => void;
}

const ContextMenuContext = React.createContext<ContextMenuContextValue | null>(
  null
);

function useContextMenu() {
  const ctx = React.useContext(ContextMenuContext);
  if (!ctx)
    throw new Error(
      "ContextMenu components must be used within <ContextMenu>"
    );
  return ctx;
}

export function ContextMenu({ children }: { children: React.ReactNode }) {
  const [open, setOpen] = React.useState(false);
  const [pos, setPos] = React.useState({ x: 0, y: 0 });

  return (
    <ContextMenuContext.Provider value={{ open, setOpen, pos, setPos }}>
      {children}
    </ContextMenuContext.Provider>
  );
}

export function ContextMenuTrigger({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) {
  const { setOpen, setPos } = useContextMenu();

  const handleContextMenu = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setPos({ x: e.clientX, y: e.clientY });
    setOpen(true);
  };

  return (
    <div className={className} onContextMenu={handleContextMenu}>
      {children}
    </div>
  );
}

export function ContextMenuContent({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) {
  const { open, setOpen, pos } = useContextMenu();
  const ref = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    if (!open) return;
    const handleClick = () => setOpen(false);
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === "Escape") setOpen(false);
    };
    document.addEventListener("mousedown", handleClick);
    document.addEventListener("keydown", handleEscape);
    return () => {
      document.removeEventListener("mousedown", handleClick);
      document.removeEventListener("keydown", handleEscape);
    };
  }, [open, setOpen]);

  if (!open) return null;

  const x = Math.min(pos.x, window.innerWidth - 200);
  const y = Math.min(pos.y, window.innerHeight - 300);

  return (
    <div
      ref={ref}
      className={cn(
        "fixed z-[100] min-w-[160px] rounded-lg border bg-background p-1 shadow-xl",
        className
      )}
      style={{ left: x, top: y }}
      onMouseDown={(e) => e.stopPropagation()}
    >
      {children}
    </div>
  );
}

export function ContextMenuItem({
  children,
  onClick,
  className,
  destructive,
}: {
  children: React.ReactNode;
  onClick?: () => void;
  className?: string;
  destructive?: boolean;
}) {
  const { setOpen } = useContextMenu();

  return (
    <button
      className={cn(
        "flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-sm cursor-pointer outline-none",
        "hover:bg-accent hover:text-accent-foreground transition-colors",
        destructive &&
          "text-destructive hover:bg-destructive/10 hover:text-destructive",
        className
      )}
      onClick={(e) => {
        e.stopPropagation();
        onClick?.();
        setOpen(false);
      }}
    >
      {children}
    </button>
  );
}

export function ContextMenuSeparator() {
  return <div className="my-1 h-px bg-border" />;
}

export function ContextMenuSub({
  children,
  label,
}: {
  children: React.ReactNode;
  label: string;
}) {
  const [open, setOpen] = React.useState(false);
  const subRef = React.useRef<HTMLDivElement>(null);

  return (
    <div
      className="relative"
      ref={subRef}
      onMouseEnter={() => setOpen(true)}
      onMouseLeave={() => setOpen(false)}
    >
      <button className="flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-sm cursor-pointer outline-none hover:bg-accent hover:text-accent-foreground transition-colors">
        <span className="flex-1 text-left">{label}</span>
        <span className="text-xs text-muted-foreground">▶</span>
      </button>
      {open && (
        <div className="absolute left-full top-0 z-[101] min-w-[160px] rounded-lg border bg-background p-1 shadow-xl">
          {children}
        </div>
      )}
    </div>
  );
}
