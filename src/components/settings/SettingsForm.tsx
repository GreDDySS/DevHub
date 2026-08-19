import { useEffect, useState } from "react";
import {
  Plus,
  Trash2,
  RefreshCw,
  FolderOpen,
  Check,
  X,
  Keyboard,
  HardDrive,
  Monitor,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useSettingsStore } from "@/stores/settingsStore";
import { invoke } from "@tauri-apps/api/core";
import { open } from "@tauri-apps/plugin-dialog";
import type { CloseAction, IdeEntry } from "@/lib/types";

function isWindows(): boolean {
  return navigator.userAgent.includes("Win");
}

const CLOSE_ACTION_LABELS: Record<CloseAction, string> = {
  Exit: "Exit App",
  MinimizeToTray: "Minimize to Tray",
  Ask: "Ask Me",
};

const CLOSE_ACTION_DESCRIPTIONS: Record<CloseAction, string> = {
  Exit: "Completely close DevHub",
  MinimizeToTray: "Hide to system tray (stay running)",
  Ask: "Show a confirmation dialog",
};

export function SettingsForm() {
  const { settings, isLoading, fetchSettings, updateSettings, restoreDefaults } =
    useSettingsStore();

  const [scanning, setScanning] = useState(false);
  const [showSaved, setShowSaved] = useState(false);
  const [pendingIde, setPendingIde] = useState<{ name: string; path: string } | null>(null);
  const [showRestoreDialog, setShowRestoreDialog] = useState(false);

  useEffect(() => {
    fetchSettings();
  }, []);

  if (isLoading || !settings) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground">
        Loading settings...
      </div>
    );
  }

  const handleSettingChange = async (
    key: keyof typeof settings,
    value: unknown
  ) => {
    await updateSettings({ [key]: value });
    setShowSaved(true);
    setTimeout(() => setShowSaved(false), 2000);
  };

  const handleAddIde = () => {
    setPendingIde({ name: "", path: "" });
  };

  const handlePendingIdeChange = (field: "name" | "path", value: string) => {
    if (!pendingIde) return;
    setPendingIde({ ...pendingIde, [field]: value });
  };

  const handleSavePendingIde = () => {
    if (!pendingIde) return;
    if (!pendingIde.name.trim() || !pendingIde.path.trim()) return;
    const newIdes = [...settings.ides, pendingIde];
    handleIdeListChange(newIdes);
    setPendingIde(null);
  };

  const handleCancelPendingIde = () => {
    setPendingIde(null);
  };

  const handleBrowseIdePath = async () => {
    try {
      const result = await open({
        title: "Select IDE executable",
        multiple: false,
        directory: false,
        ...(isWindows() ? { filters: [{ name: "Executables", extensions: ["exe"] }] } : {}),
      });

      if (result && typeof result === "string") {
        const exeName = result
          .split(/[\\/]/)
          .pop()
          ?.replace(/\.(exe|app)$/i, "") || "IDE";
        setPendingIde({ name: exeName, path: result });
      }
    } catch (e) {
      console.error("Failed to open file dialog:", e);
    }
  };

  const handleRemoveIde = (index: number) => {
    const newIdes = settings.ides.filter((_, i) => i !== index);
    handleIdeListChange(newIdes);
  };

  const handleIdeChange = (index: number, field: "name" | "path", value: string) => {
    const newIdes = [...settings.ides];
    newIdes[index] = { ...newIdes[index], [field]: value };
    handleIdeListChange(newIdes);
  };

  const handleIdeListChange = (
    ides: IdeEntry[],
    extra?: Partial<typeof settings>
  ) => {
    handleSettingChange("ides", ides);
    if (extra?.default_ide_index !== undefined) {
      handleSettingChange("default_ide_index", extra.default_ide_index);
    }
  };

  const handleScanIdes = async () => {
    setScanning(true);
    try {
      const scanned = await invoke<IdeEntry[]>("scan_ides");
      const existingPaths = new Set(settings.ides.map((ide) => ide.path));
      const newIdes = scanned.filter((ide) => !existingPaths.has(ide.path));
      if (newIdes.length > 0) {
        const merged = [...settings.ides, ...newIdes];
        handleIdeListChange(merged);
      }
    } catch (e) {
      console.error("Failed to scan IDEs:", e);
    } finally {
      setScanning(false);
    }
  };

  const handleRestoreDefaults = async () => {
    await restoreDefaults();
    setShowSaved(true);
    setTimeout(() => setShowSaved(false), 2000);
    setShowRestoreDialog(false);
  };

  const validateIdePath = (path: string): boolean => {
    return path.trim().length > 0;
  };

  const validIdes = settings.ides.filter((ide) => validateIdePath(ide.path));

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Settings</h1>
        {showSaved && (
          <Badge variant="secondary" className="flex items-center gap-1">
            <Check className="h-3 w-3" />
            Saved
          </Badge>
        )}
      </div>

      <div className="space-y-6 flex-1 overflow-y-auto">
        {/* Appearance */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Monitor className="h-5 w-5" />
              Appearance
            </CardTitle>
            <CardDescription>
              Customize the look and feel of DevHub
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between">
              <div className="space-y-1">
                <p className="font-medium">Dark Theme</p>
                <p className="text-sm text-muted-foreground">
                  Use a dark color scheme
                </p>
              </div>
              <Switch
                checked={settings.is_dark_theme}
                onCheckedChange={() => handleSettingChange("is_dark_theme", !settings.is_dark_theme)}
              />
            </div>

            <div className="flex items-center justify-between">
              <div className="space-y-1">
                <p className="font-medium">Project Statuses</p>
                <p className="text-sm text-muted-foreground">
                  Show active/inactive status badges on project cards
                </p>
              </div>
              <Switch
                checked={settings.statuses_enabled}
                onCheckedChange={() =>
                  handleSettingChange("statuses_enabled", !settings.statuses_enabled)
                }
              />
            </div>
          </CardContent>
        </Card>

        {/* Behavior */}
        <Card>
          <CardHeader>
            <CardTitle>Behavior</CardTitle>
            <CardDescription>
              How DevHub behaves on startup and shutdown
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between">
              <div className="space-y-1">
                <p className="font-medium">Autostart</p>
                <p className="text-sm text-muted-foreground">
                  Launch DevHub automatically when you log in
                </p>
              </div>
              <Switch
                checked={settings.autostart_enabled}
                onCheckedChange={() =>
                  handleSettingChange(
                    "autostart_enabled",
                    !settings.autostart_enabled
                  )
                }
              />
            </div>

            <div className="space-y-2">
              <p className="font-medium">Close Action</p>
              <p className="text-sm text-muted-foreground">
                What happens when you close the main window
              </p>
              <div className="flex flex-col gap-2 mt-2">
                {(
                  ["Exit", "MinimizeToTray", "Ask"] as CloseAction[]
                ).map((action) => (
                  <button
                    key={action}
                    onClick={() => handleSettingChange("close_action", action)}
                    className={`
                      flex items-start gap-3 rounded-lg border p-3 text-left
                      transition-all
                      ${
                        settings.close_action === action
                          ? "border-primary bg-accent/50"
                          : "border-border hover:bg-accent/30"
                      }
                    `}
                  >
                    <div
                      className={`
                        mt-0.5 h-4 w-4 rounded-full
                        ${
                          settings.close_action === action
                            ? "bg-primary"
                            : "bg-muted"
                        }
                      `}
                    />
                    <div className="flex-1">
                      <p className="font-medium">
                        {CLOSE_ACTION_LABELS[action]}
                      </p>
                      <p className="text-sm text-muted-foreground">
                        {CLOSE_ACTION_DESCRIPTIONS[action]}
                      </p>
                    </div>
                  </button>
                ))}
              </div>
            </div>

            <div className="flex items-center justify-between">
              <div className="space-y-1">
                <p className="font-medium">Inactive After (days)</p>
                <p className="text-sm text-muted-foreground">
                  Days of inactivity before a project is marked Inactive
                </p>
              </div>
              <Input
                type="number"
                min={1}
                max={365}
                value={settings.inactive_days}
                onChange={(e) =>
                  handleSettingChange(
                    "inactive_days",
                    Math.max(1, Math.min(365, parseInt(e.target.value) || 30))
                  )
                }
                className="w-20 text-right"
              />
            </div>
          </CardContent>
        </Card>

        {/* IDEs */}
        <Card>
          <CardHeader>
            <CardTitle>IDEs</CardTitle>
            <CardDescription>
              Configure IDEs for launching projects
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {validIdes.length === 0 && !pendingIde ? (
              <div className="text-center py-8 border-2 border-dashed border-border rounded-lg">
                <FolderOpen className="h-8 w-8 mx-auto text-muted-foreground mb-2" />
                <p className="text-sm text-muted-foreground">
                  No IDEs configured yet
                </p>
                <p className="text-xs text-muted-foreground mt-1">
                  Scan for installed IDEs or add one manually
                </p>
              </div>
            ) : (
              <div className="space-y-3">
                {settings.ides.map((ide, index) => (
                  <div key={index} className="flex items-center gap-2">
                    <div className="flex-1">
                      <Input
                        placeholder="IDE Name"
                        value={ide.name}
                        onChange={(e) =>
                          handleIdeChange(index, "name", e.target.value)
                        }
                      />
                    </div>
                    <div className="flex-1">
                      <div className="relative">
                        <Input
                          placeholder="IDE Path"
                          value={ide.path}
                          onChange={(e) =>
                            handleIdeChange(index, "path", e.target.value)
                          }
                        />
                        <div className="absolute right-2 top-1/2 -translate-y-1/2">
                          {validateIdePath(ide.path) ? (
                            <Check className="h-3 w-3 text-green-500" />
                          ) : (
                            <X className="h-3 w-3 text-red-500" />
                          )}
                        </div>
                      </div>
                    </div>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleRemoveIde(index)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                ))}
              </div>
            )}

            <div className="flex gap-2">
              <Button variant="outline" onClick={handleAddIde}>
                <Plus className="h-4 w-4 mr-2" />
                Add IDE
              </Button>
              <Button
                variant="outline"
                onClick={handleScanIdes}
                disabled={scanning}
              >
                <RefreshCw
                  className={`h-4 w-4 mr-2 ${scanning ? "animate-spin" : ""}`}
                />
                {scanning ? "Scanning..." : "Scan for IDEs"}
              </Button>
            </div>

            {pendingIde && (
              <Dialog open={true} onOpenChange={() => handleCancelPendingIde()}>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Add IDE</DialogTitle>
                    <DialogDescription>
                      Configure a new IDE entry
                    </DialogDescription>
                  </DialogHeader>
                  <div className="space-y-3 py-2">
                    <Input
                      placeholder="IDE Name"
                      value={pendingIde.name}
                      onChange={(e) =>
                        handlePendingIdeChange("name", e.target.value)
                      }
                    />
                    <div className="flex gap-2">
                      <Input
                        placeholder="IDE Path"
                        value={pendingIde.path}
                        onChange={(e) =>
                          handlePendingIdeChange("path", e.target.value)
                        }
                      />
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={handleBrowseIdePath}
                      >
                        Browse
                      </Button>
                    </div>
                  </div>
                  <DialogFooter>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={handleCancelPendingIde}
                    >
                      Cancel
                    </Button>
                    <Button
                      size="sm"
                      onClick={handleSavePendingIde}
                      disabled={
                        !pendingIde.name.trim() || !pendingIde.path.trim()
                      }
                    >
                      Add
                    </Button>
                   </DialogFooter>
                 </DialogContent>
               </Dialog>
               )}
          </CardContent>
        </Card>

        {/* Keyboard Shortcuts */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Keyboard className="h-5 w-5" />
              Keyboard Shortcuts
            </CardTitle>
            <CardDescription>
              Global shortcuts for quick actions
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="space-y-1">
              <div className="flex items-center justify-between">
                <kbd className="px-2 py-1 text-xs bg-muted rounded">
                  Ctrl + Shift + Y
                </kbd>
                <span className="text-sm text-muted-foreground">
                  Capture URL from clipboard
                </span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Data Management */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <HardDrive className="h-5 w-5" />
              Data Management
            </CardTitle>
            <CardDescription>
              Storage location and bulk actions
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between">
              <div className="space-y-1">
                <p className="font-medium">Data Directory</p>
                <p className="text-sm text-muted-foreground">
                  Local storage location for your projects and links
                </p>
              </div>
              <Button
                variant="outline"
                size="sm"
                onClick={async () => {
                  try {
                    const dataDir = await invoke<string>("get_data_dir");
                    await invoke("open_in_explorer", { path: dataDir });
                  } catch (e) {
                    console.error("Failed to open data dir:", e);
                  }
                }}
              >
                <FolderOpen className="h-4 w-4 mr-2" />
                Open Folder
              </Button>
            </div>

            <div className="border-t pt-4">
              <div className="flex items-center justify-between">
                <div className="space-y-1">
                  <p className="font-medium text-destructive">
                    Restore Default Settings
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Reset all settings to factory defaults. This does not
                    delete your projects or links.
                  </p>
                </div>
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={() => setShowRestoreDialog(true)}
                >
                  Restore Defaults
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Dialog open={showRestoreDialog} onOpenChange={setShowRestoreDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Restore Default Settings?</DialogTitle>
            <DialogDescription>
              This will reset all settings to their factory defaults. Your
              projects and links will not be affected.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setShowRestoreDialog(false)}
            >
              Cancel
            </Button>
            <Button variant="destructive" size="sm" onClick={handleRestoreDefaults}>
              Restore Defaults
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
