import * as React from "react";
import { Check, ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Popover, PopoverTrigger, PopoverContent } from "@/components/ui/popover";
import { cn } from "@/lib/utils";

export interface ComboboxOption {
  label: string;
  value: string;
  icon?: React.ReactNode;
}

export function Combobox({
  options,
  value,
  onChange,
  placeholder = "Select...",
  className,
}: {
  options: ComboboxOption[];
  value: string | null | undefined;
  onChange: (value: string | null) => void;
  placeholder?: string;
  className?: string;
}) {
  const [open, setOpen] = React.useState(false);

  const selected = options.find((o) => o.value === value);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          className={cn("justify-between gap-2 min-w-[120px]", className)}
        >
          <span className="flex items-center gap-1.5 truncate">
            {selected?.icon}
            {selected?.label ?? placeholder}
          </span>
          <ChevronDown className="h-3.5 w-3.5 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent align="start" className="w-[180px] p-0">
        <div className="py-1">
          {options.map((option) => (
            <button
              key={option.value}
              className={cn(
                "flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-sm cursor-pointer outline-none hover:bg-accent hover:text-accent-foreground",
                value === option.value && "bg-accent"
              )}
              onClick={() => {
                onChange(option.value === value ? null : option.value);
                setOpen(false);
              }}
            >
              {option.icon}
              <span className="flex-1 text-left">{option.label}</span>
              {value === option.value && (
                <Check className="h-3.5 w-3.5 shrink-0" />
              )}
            </button>
          ))}
        </div>
      </PopoverContent>
    </Popover>
  );
}
