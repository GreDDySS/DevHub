import React, { useState } from "react";
import { X, Link, Loader2, CheckCircle, AlertCircle, Clipboard } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useLinkStore } from "@/stores/linkStore";

interface CaptureLinkDialogProps {
  onClose: () => void;
}

export function CaptureLinkDialog({ onClose }: CaptureLinkDialogProps) {
  const { captureLink, addLinkFromClipboard } = useLinkStore();
  const [url, setUrl] = useState("");
  const [isCapturing, setIsCapturing] = useState(false);
  const [result, setResult] = useState<{ success: boolean; message: string } | null>(null);

  const handleCaptureFromClipboard = async () => {
    setIsCapturing(true);
    setResult(null);

    try {
      const link = await addLinkFromClipboard();
      if (link) {
        setResult({ success: true, message: `Captured: ${link.title || link.url}` });
        setTimeout(() => onClose(), 1500);
      } else {
        setResult({ success: false, message: "No valid link found in clipboard" });
      }
    } catch (e) {
      setResult({ success: false, message: String(e) });
    } finally {
      setIsCapturing(false);
    }
  };

  const handleCaptureFromUrl = async () => {
    if (!url.trim()) {
      setResult({ success: false, message: "Please enter a URL" });
      return;
    }

    setIsCapturing(true);
    setResult(null);

    try {
      const link = await captureLink(url.trim());
      if (link) {
        setResult({ success: true, message: `Captured: ${link.title || link.url}` });
        setTimeout(() => onClose(), 1500);
      } else {
        setResult({ success: false, message: "Failed to capture link" });
      }
    } catch (e) {
      setResult({ success: false, message: String(e) });
    } finally {
      setIsCapturing(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <Card className="w-full max-w-md mx-4">
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>Capture Link</CardTitle>
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <p className="text-sm text-muted-foreground">
              Capture a link from your clipboard
            </p>
            <Button
              onClick={handleCaptureFromClipboard}
              disabled={isCapturing}
              className="w-full"
            >
              {isCapturing ? (
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
              ) : (
                <Clipboard className="h-4 w-4 mr-2" />
              )}
              {isCapturing ? "Capturing..." : "Capture from Clipboard"}
            </Button>
          </div>

          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <span className="w-full border-t" />
            </div>
            <div className="relative flex justify-center text-xs uppercase">
              <span className="bg-card px-2 text-muted-foreground">or</span>
            </div>
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium">Enter URL manually</label>
            <div className="flex gap-2">
              <Input
                placeholder="https://example.com"
                value={url}
                onChange={(e) => setUrl(e.target.value)}
                className="flex-1"
                onKeyDown={(e) => {
                  if (e.key === "Enter") handleCaptureFromUrl();
                }}
              />
              <Button
                variant="outline"
                onClick={handleCaptureFromUrl}
                disabled={isCapturing || !url.trim()}
              >
                Add
              </Button>
            </div>
          </div>

          {result && (
            <div
              className={`flex items-center gap-2 p-3 rounded-md text-sm ${
                result.success
                  ? "bg-green-500/10 text-green-600"
                  : "bg-destructive/10 text-destructive"
              }`}
            >
              {result.success ? (
                <CheckCircle className="h-4 w-4" />
              ) : (
                <AlertCircle className="h-4 w-4" />
              )}
              {result.message}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
