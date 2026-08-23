import { useEffect } from "react";
import { GitBranch, RefreshCw } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useGitStore } from "@/stores/gitStore";
import { formatRelativeTime, cn } from "@/lib/utils";

interface GitActivityProps {
  projectPath: string;
}

export function GitActivitySection({ projectPath }: GitActivityProps) {
  const { activity, isLoading, fetchGitActivity, clearGitActivity } =
    useGitStore();

  useEffect(() => {
    clearGitActivity();
    fetchGitActivity(projectPath);
    return () => clearGitActivity();
  }, [projectPath, fetchGitActivity, clearGitActivity]);

  if (isLoading && !activity) {
    return (
      <div className="flex items-center gap-2 py-4 text-xs text-muted-foreground">
        <GitBranch className="h-4 w-4 animate-pulse" />
        Reading git history...
      </div>
    );
  }

  if (!activity || activity.commits.length === 0) return null;

  return (
    <div className="flex flex-col gap-3">
      <div className="flex items-center gap-2">
        <GitBranch className="h-4 w-4 text-muted-foreground" />
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          GIT ACTIVITY
        </h2>
        <Badge variant="secondary" className="text-[10px] px-1.5 h-4.5">
          {activity.branch}
        </Badge>
        {activity.total_commits > 0 && (
          <span className="text-xs text-muted-foreground">
            {activity.total_commits} commits
          </span>
        )}
        <Button
          variant="ghost"
          size="icon"
          className="h-6 w-6 ml-auto opacity-40 hover:opacity-100 transition-opacity"
          onClick={() => fetchGitActivity(projectPath)}
          disabled={isLoading}
          title="Refresh git activity"
        >
          <RefreshCw className={cn("h-3.5 w-3.5", isLoading && "animate-spin")} />
        </Button>
      </div>

      <div className="flex flex-col">
        {activity.commits.map((commit, i) => (
          <div
            key={commit.hash}
            className="group flex items-baseline gap-3 rounded-lg px-2 py-1.5 hover:bg-accent/50 transition-colors"
          >
            <span className="font-mono text-xs font-medium text-amber-600 dark:text-amber-400 shrink-0 w-14 truncate">
              {commit.short_hash}
            </span>
            <span
              className="text-sm min-w-0 truncate flex-1"
              title={commit.message}
            >
              {commit.message}
            </span>
            <span className="text-xs text-muted-foreground shrink-0 hidden sm:inline max-w-40 truncate">
              {commit.author}
            </span>
            <span className="text-xs text-sky-600 dark:text-sky-400 shrink-0 w-16 text-right">
              {formatRelativeTime(commit.timestamp)}
            </span>
            {i === 0 && activity.commits.length > 1 && (
              <Badge
                variant="secondary"
                className="text-[10px] px-1.5 h-4.5 shrink-0 bg-green-500/10 text-green-600 dark:text-green-400"
              >
                HEAD
              </Badge>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
