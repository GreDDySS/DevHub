import React, { useEffect } from "react";
import { Save, Plus, Trash2, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { useSettingsStore } from "@/stores/settingsStore";
import type { CloseAction } from "@/lib/types";
import { cn } from "@/lib/utils";

export function SettingsForm() {
  const { settings, isLoading, fetchSettings, saveSettings } =
    useSettingsStore();

  useEffect(() => {
    fetchSettings();
  }, []);

  if (isLoading || !settings) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground">
        Loading...
      </div>
    );
  }

  const handleSave = () => {
    saveSettings(settings);
  };

  const handleAddIde = () => {
    const newIdes = [...settings.ides, { name: "", path: "" }];
    saveSettings({ ...settings, ides: newIdes });
  };

  const handleRemoveIde = (index: number) => {
    const newIdes = settings.ides.filter((_, i) => i !== index);
    saveSettings({ ...settings, ides: newIdes });
  };

  const handleIdeChange = (
    index: number,
    field: "name" | "path",
    value: string
  ) => {
    const newIdes = [...settings.ides];
    newIdes[index] = { ...newIdes[index], [field]: value };
    saveSettings({ ...settings, ides: newIdes });
  };

  const handleCloseActionChange = (action: CloseAction) => {
    saveSettings({ ...settings, close_action: action });
  };

  const handleAutostartToggle = () => {
    saveSettings({ ...settings, autostart_enabled: !settings.autostart_enabled });
  };

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Settings</h1>
        <Button size="sm" onClick={handleSave}>
          <Save className="h-4 w-4 mr-2" />
          Save
        </Button>
      </div>

      <div className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>IDEs</CardTitle>
            <CardDescription>Configure your preferred IDEs</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {settings.ides.map((ide, index) => (
              <div key={index} className="flex items-center gap-2">
                <Input
                  placeholder="IDE Name"
                  value={ide.name}
                  onChange={(e) =>
                    handleIdeChange(index, "name", e.target.value)
                  }
                  className="w-[150px]"
                />
                <Input
                  placeholder="IDE Path"
                  value={ide.path}
                  onChange={(e) =>
                    handleIdeChange(index, "path", e.target.value)
                  }
                  className="flex-1"
                />
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => handleRemoveIde(index)}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            ))}
            <Button variant="outline" onClick={handleAddIde}>
              <Plus className="h-4 w-4 mr-2" />
              Add IDE
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Close Action</CardTitle>
            <CardDescription>
              What happens when you close the window
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex gap-2">
              {(["Exit", "MinimizeToTray", "Ask"] as CloseAction[]).map(
                (action) => (
                  <Button
                    key={action}
                    variant={
                      settings.close_action === action ? "default" : "outline"
                    }
                    onClick={() => handleCloseActionChange(action)}
                  >
                    {action === "MinimizeToTray"
                      ? "Minimize to Tray"
                      : action}
                  </Button>
                )
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>General</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="font-medium">Autostart</p>
                <p className="text-sm text-muted-foreground">
                  Start DevHub when you log in
                </p>
              </div>
              <Button
                variant={settings.autostart_enabled ? "default" : "outline"}
                onClick={handleAutostartToggle}
              >
                {settings.autostart_enabled ? "Enabled" : "Disabled"}
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
