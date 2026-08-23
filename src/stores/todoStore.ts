import { create } from "zustand";
import { invoke } from "@tauri-apps/api/core";
import type { Todo, UpdateTodoRequest, TodoPriority } from "@/lib/types";

interface TodoState {
  todos: Todo[];
  isLoading: boolean;
  error: string | null;

  fetchTodos: (projectId: string | null) => Promise<void>;
  addTodo: (title: string, projectId: string | null) => Promise<Todo | null>;
  toggleTodo: (id: string) => Promise<void>;
  updateTodo: (
    id: string,
    request: UpdateTodoRequest
  ) => Promise<Todo | null>;
  cyclePriority: (id: string, current: TodoPriority) => Promise<void>;
  deleteTodo: (id: string) => Promise<boolean>;
  clearCompleted: (projectId: string | null) => Promise<void>;
}

const PRIORITY_CYCLE: Record<TodoPriority, TodoPriority> = {
  Low: "Normal",
  Normal: "High",
  High: "Low",
};

export const useTodoStore = create<TodoState>((set, get) => ({
  todos: [],
  isLoading: false,
  error: null,

  fetchTodos: async (projectId) => {
    set({ isLoading: true, error: null });
    try {
      const todos = await invoke<Todo[]>("get_todos", { projectId });
      set({ todos, isLoading: false });
    } catch (error) {
      set({ error: String(error), isLoading: false });
    }
  },

  addTodo: async (title, projectId) => {
    try {
      const todo = await invoke<Todo>("add_todo", { title, projectId });
      await get().fetchTodos(projectId);
      return todo;
    } catch (error) {
      set({ error: String(error) });
      return null;
    }
  },

  toggleTodo: async (id) => {
    try {
      await invoke("toggle_todo", { id });
      set((state) => ({
        todos: state.todos.map((t) =>
          t.id === id
            ? {
                ...t,
                is_completed: !t.is_completed,
                completed_at: !t.is_completed ? new Date().toISOString() : null,
              }
            : t
        ),
      }));
    } catch (error) {
      set({ error: String(error) });
    }
  },

  updateTodo: async (id, request) => {
    try {
      const todo = await invoke<Todo>("update_todo", { id, request });
      set((state) => ({
        todos: state.todos.map((t) => (t.id === id ? todo : t)),
      }));
      return todo;
    } catch (error) {
      set({ error: String(error) });
      return null;
    }
  },

  cyclePriority: async (id, current) => {
    await get().updateTodo(id, { priority: PRIORITY_CYCLE[current] });
  },

  deleteTodo: async (id) => {
    try {
      await invoke("delete_todo", { id });
      set((state) => ({ todos: state.todos.filter((t) => t.id !== id) }));
      return true;
    } catch (error) {
      set({ error: String(error) });
      return false;
    }
  },

  clearCompleted: async (projectId) => {
    try {
      await invoke("clear_completed_todos");
      await get().fetchTodos(projectId);
    } catch (error) {
      set({ error: String(error) });
    }
  },
}));
