import React, { useEffect, useMemo, useRef, useState } from "react";
import { Plus, Trash2, Check, ListTodo } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { useTodoStore } from "@/stores/todoStore";
import type { Todo } from "@/lib/types";
import { PRIORITY_COLORS } from "@/lib/types";
import { cn } from "@/lib/utils";

type Filter = "all" | "active" | "completed";

interface TodoListProps {
  projectId: string;
}

export function TodoList({ projectId }: TodoListProps) {
  const {
    todos,
    fetchTodos,
    addTodo,
    toggleTodo,
    updateTodo,
    cyclePriority,
    deleteTodo,
    clearCompleted,
  } = useTodoStore();
  const [newTitle, setNewTitle] = useState("");
  const [showInput, setShowInput] = useState(false);
  const [filter, setFilter] = useState<Filter>("all");
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editValue, setEditValue] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);
  const editInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    fetchTodos(projectId);
  }, [projectId, fetchTodos]);

  useEffect(() => {
    if (showInput) {
      inputRef.current?.focus();
    }
  }, [showInput]);

  useEffect(() => {
    if (editingId) {
      editInputRef.current?.focus();
      editInputRef.current?.select();
    }
  }, [editingId]);

  const isRowEditing =
    !!editingId && todos.some((t) => t.id === editingId);

  const stats = useMemo(() => {
    const total = todos.length;
    const completed = todos.filter((t) => t.is_completed).length;
    return {
      total,
      completed,
      active: total - completed,
      percent: total === 0 ? 0 : Math.round((completed / total) * 100),
    };
  }, [todos]);

  const visible = useMemo(() => {
    switch (filter) {
      case "active":
        return todos.filter((t) => !t.is_completed);
      case "completed":
        return todos.filter((t) => t.is_completed);
      default:
        return todos;
    }
  }, [todos, filter]);

  const handleAdd = async () => {
    const title = newTitle.trim();
    if (!title) return;
    await addTodo(title, projectId);
    setNewTitle("");
    inputRef.current?.focus();
  };

  const closeInput = () => {
    setShowInput(false);
    setNewTitle("");
  };

  const startEdit = (todo: Todo) => {
    setEditingId(todo.id);
    setEditValue(todo.title);
  };

  const commitEdit = async () => {
    if (!editingId) return;
    const title = editValue.trim();
    if (title) {
      await updateTodo(editingId, { title });
    }
    setEditingId(null);
  };

  const cancelEdit = () => {
    setEditingId(null);
    setEditValue("");
  };

  const filters: { key: Filter; label: string; count: number }[] = [
    { key: "all", label: "All", count: stats.total },
    { key: "active", label: "Active", count: stats.active },
    { key: "completed", label: "Done", count: stats.completed },
  ];

  return (
    <div className="group flex flex-col gap-3">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 min-w-0">
          <ListTodo className="h-4 w-4 text-muted-foreground" />
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            TODO
          </h2>
          <span className="text-xs text-muted-foreground">
            {stats.completed}/{stats.total}
          </span>
        </div>
        <div className="flex items-center gap-1 shrink-0">
          <Button
            variant="ghost"
            size="sm"
            className={cn(
              "h-7 px-1.5 text-muted-foreground transition-opacity",
              stats.completed > 0
                ? "opacity-100"
                : "opacity-0 pointer-events-none"
            )}
            onClick={() => clearCompleted(projectId)}
            title="Clear completed"
          >
            <Trash2 className="h-3 w-3" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className={cn(
              "h-6 w-6 transition-opacity",
              isRowEditing
                ? "opacity-0 pointer-events-none"
                : "opacity-40 group-hover:opacity-100"
            )}
            onClick={() => (showInput ? closeInput() : setShowInput(true))}
            title="Add task"
          >
            <Plus className="h-3.5 w-3.5" />
          </Button>
        </div>
      </div>

      {showInput && (
        <div onBlur={(e) => !e.currentTarget.contains(e.relatedTarget as Node) && !newTitle.trim() && closeInput()}>
          <Input
            ref={inputRef}
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") handleAdd();
              if (e.key === "Escape") closeInput();
            }}
            placeholder="What needs to be done? (Enter)"
            className="h-8 text-sm"
            autoFocus
          />
        </div>
      )}

      {stats.total > 0 && (
        <div className="h-1 w-full rounded-full bg-secondary overflow-hidden">
          <div
            className={cn(
              "h-full rounded-full transition-all duration-300",
              stats.percent === 100 ? "bg-green-500" : "bg-primary"
            )}
            style={{ width: `${stats.percent}%` }}
          />
        </div>
      )}

      {stats.total > 0 && (
        <div className="flex items-center gap-1">
          {filters.map((f) => (
            <Button
              key={f.key}
              variant={filter === f.key ? "secondary" : "ghost"}
              size="sm"
              className="h-7 text-xs"
              onClick={() => setFilter(f.key)}
            >
              {f.label}
              <span className="ml-1.5 text-muted-foreground">{f.count}</span>
            </Button>
          ))}
        </div>
      )}

      <div className="flex flex-col gap-1 h-[280px] overflow-y-auto pr-1">
        {visible.length === 0 ? (
          <div className="flex-1 flex items-center justify-center py-6">
            <p className="text-xs text-muted-foreground text-center px-4">
              {filter !== "all"
                ? "Nothing here."
                : showInput
                  ? "Type a task and press Enter."
                  : "No tasks yet. Hover the header and press + to add one."}
            </p>
          </div>
        ) : (
          visible.map((todo) => (
            <div
              key={todo.id}
              onDoubleClick={() => !todo.is_completed && startEdit(todo)}
              className={cn(
                "group/item flex items-center gap-2 rounded-lg px-2 py-1.5 transition-colors",
                todo.is_completed ? "opacity-60" : "hover:bg-accent/50"
              )}
            >
              <button
                onClick={() => toggleTodo(todo.id)}
                className={cn(
                  "flex items-center justify-center h-4.5 w-4.5 shrink-0 rounded-md border transition-colors cursor-pointer",
                  todo.is_completed
                    ? "bg-green-500 border-green-500 text-white"
                    : "border-muted-foreground/40 hover:border-primary"
                )}
                title={
                  todo.is_completed ? "Mark as active" : "Mark as done"
                }
              >
                {todo.is_completed && <Check className="h-3 w-3" />}
              </button>

              {editingId === todo.id ? (
                <Input
                  ref={editInputRef}
                  value={editValue}
                  onChange={(e) => setEditValue(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") commitEdit();
                    if (e.key === "Escape") cancelEdit();
                  }}
                  onBlur={commitEdit}
                  className="h-6 py-0 text-sm flex-1"
                />
              ) : (
                <span
                  className={cn(
                    "text-sm flex-1 min-w-0 break-words leading-snug",
                    todo.is_completed &&
                      "line-through text-muted-foreground"
                  )}
                  title="Double-click to edit"
                >
                  {todo.title}
                </span>
              )}

              {editingId !== todo.id && !todo.is_completed && (
                <Badge
                  variant="secondary"
                  className={cn(
                    "text-[10px] px-1.5 h-4.5 shrink-0 cursor-pointer select-none opacity-60 group-hover/item:opacity-100",
                    PRIORITY_COLORS[todo.priority]
                  )}
                  onClick={() => cyclePriority(todo.id, todo.priority)}
                  title="Click to change priority"
                >
                  {todo.priority}
                </Badge>
              )}

              {editingId !== todo.id && (
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-6 w-6 shrink-0 opacity-30 group-hover/item:opacity-80 hover:!opacity-100 hover:text-destructive"
                  onClick={() => deleteTodo(todo.id)}
                >
                  <Trash2 className="h-3.5 w-3.5" />
                </Button>
              )}
            </div>
          ))
        )}
      </div>
    </div>
  );
}
