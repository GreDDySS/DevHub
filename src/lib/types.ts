export type ProjectStatus = "Active" | "Completed" | "Paused" | "Archived";

export type ProgrammingLanguage =
  | "CSharp"
  | "Python"
  | "Rust"
  | "JavaScript"
  | "TypeScript"
  | "Go"
  | "Java"
  | "Cpp"
  | "Other";

export type LinkType =
  | "YouTube"
  | "Article"
  | "Repository"
  | "Documentation"
  | "Other";

export type CloseAction = "Exit" | "MinimizeToTray" | "Ask";

export interface Project {
  id: string;
  name: string;
  path: string;
  description: string;
  language: ProgrammingLanguage;
  status: ProjectStatus;
  tags: string[];
  preferred_ide: string | null;
  is_favorite: boolean;
  is_hidden: boolean;
  created_at: string;
  updated_at: string;
}

export interface Link {
  id: string;
  url: string;
  title: string;
  type: LinkType;
  project_id: string | null;
  tags: string[];
  notes: string;
  captured_at: string;
  created_at: string;
  updated_at: string;
}

export interface IdeEntry {
  name: string;
  path: string;
}

export interface AppSettings {
  version: number;
  ides: IdeEntry[];
  default_ide_index: number;
  autostart_enabled: boolean;
  close_action: CloseAction;
  is_dark_theme: boolean;
}

export interface ProjectFilter {
  search_query?: string;
  status?: ProjectStatus | null;
  languages?: ProgrammingLanguage[];
  sort_by?: string;
  tags?: string[];
  show_hidden?: boolean;
}

export interface CreateProjectRequest {
  name: string;
  path: string;
  description?: string;
  language?: ProgrammingLanguage;
}

export interface UpdateProjectRequest {
  name?: string;
  path?: string;
  description?: string;
  language?: ProgrammingLanguage;
  status?: ProjectStatus;
  tags?: string[];
  preferred_ide?: string | null;
  is_favorite?: boolean;
  is_hidden?: boolean;
}

export const LANGUAGE_ICONS: Record<ProgrammingLanguage, string> = {
  CSharp: "C#",
  Python: "Py",
  Rust: "Rs",
  JavaScript: "JS",
  TypeScript: "TS",
  Go: "Go",
  Java: "Jv",
  Cpp: "C+",
  Other: "??",
};

export const LANGUAGE_COLORS: Record<ProgrammingLanguage, string> = {
  CSharp: "bg-purple-500/15 text-purple-600 dark:text-purple-400",
  Python: "bg-blue-500/15 text-blue-600 dark:text-blue-400",
  Rust: "bg-orange-500/15 text-orange-600 dark:text-orange-400",
  JavaScript: "bg-yellow-500/15 text-yellow-600 dark:text-yellow-400",
  TypeScript: "bg-blue-500/15 text-blue-600 dark:text-blue-400",
  Go: "bg-cyan-500/15 text-cyan-600 dark:text-cyan-400",
  Java: "bg-red-500/15 text-red-600 dark:text-red-400",
  Cpp: "bg-blue-500/15 text-blue-600 dark:text-blue-400",
  Other: "bg-gray-500/15 text-gray-600 dark:text-gray-400",
};

export const STATUS_COLORS: Record<ProjectStatus, string> = {
  Active: "bg-green-500/10 text-green-600 dark:text-green-400",
  Completed: "bg-blue-500/10 text-blue-600 dark:text-blue-400",
  Paused: "bg-yellow-500/10 text-yellow-600 dark:text-yellow-400",
  Archived: "bg-gray-500/10 text-gray-600 dark:text-gray-400",
};

export const LINK_TYPE_COLORS: Record<LinkType, string> = {
  YouTube: "bg-red-500/10 text-red-600 dark:text-red-400",
  Repository: "bg-gray-500/10 text-gray-600 dark:text-gray-400",
  Article: "bg-blue-500/10 text-blue-600 dark:text-blue-400",
  Documentation: "bg-green-500/10 text-green-600 dark:text-green-400",
  Other: "bg-purple-500/10 text-purple-600 dark:text-purple-400",
};
