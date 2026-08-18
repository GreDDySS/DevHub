import * as React from "react";
import { Check, ChevronDown, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Popover, PopoverTrigger, PopoverContent } from "@/components/ui/popover";
import { cn } from "@/lib/utils";

export interface MultiComboboxOption {
  label: string;
  value: string;
  icon?: React.ReactNode;
}

export function MultiCombobox({
  options,
  values,
  onChange,
  placeholder = "Select...",
  className,
}: {
  options: MultiComboboxOption[];
  values: string[];
  onChange: (values: string[]) => void;
  placeholder?: string;
  className?: string;
}) {
  const [open, setOpen] = React.useState(false);

  const toggle = (val: string) => {
    if (values.includes(val)) {
      onChange(values.filter((v) => v !== val));
    } else {
      onChange([...values, val]);
    }
  };

  const selectedCount = values.length;

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          className={cn("justify-between gap-2 min-w-[120px]", className)}
        >
          <span className="truncate">
            {selectedCount > 0
              ? `${selectedCount} language${selectedCount > 1 ? "s" : ""}`
              : placeholder}
          </span>
          <ChevronDown className="h-3.5 w-3.5 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent align="start" className="w-[200px] p-0">
        <div className="max-h-[240px] overflow-auto py-1">
          {options.map((option) => {
            const isSelected = values.includes(option.value);
            return (
              <button
                key={option.value}
                className={cn(
                  "flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-sm cursor-pointer outline-none hover:bg-accent hover:text-accent-foreground",
                  isSelected && "bg-accent"
                )}
                onClick={() => toggle(option.value)}
              >
                <div
                  className={cn(
                    "flex h-4 w-4 items-center justify-center rounded-sm border shrink-0",
                    isSelected
                      ? "bg-primary border-primary text-primary-foreground"
                      : "border-input"
                  )}
                >
                  {isSelected && <Check className="h-3 w-3" />}
                </div>
                {option.icon}
                <span className="flex-1 text-left">{option.label}</span>
              </button>
            );
          })}
        </div>
        {selectedCount > 0 && (
          <div className="border-t px-2 py-1.5">
            <button
              className="text-xs text-muted-foreground hover:text-foreground cursor-pointer"
              onClick={() => {
                onChange([]);
              }}
            >
              Clear all
            </button>
          </div>
        )}
      </PopoverContent>
    </Popover>
  );
}
