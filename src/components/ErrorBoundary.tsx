import React, { Component } from "react";
import { AlertTriangle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface Props {
  children: React.ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error("ErrorBoundary caught:", error, errorInfo);
  }

  handleReload = () => {
    window.location.reload();
  };

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex items-center justify-center min-h-screen bg-background p-4">
          <Card className="w-full max-w-md">
            <CardHeader className="flex flex-row items-center gap-3 pb-3">
              <AlertTriangle className="h-6 w-6 text-destructive" />
              <CardTitle className="text-base">Something went wrong</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-muted-foreground">
                An unexpected error occurred. You can try reloading the
                application.
              </p>
              {this.state.error && (
                <pre className="text-xs text-muted-foreground bg-muted p-3 rounded overflow-auto max-h-32">
                  {this.state.error.message}
                </pre>
              )}
              <Button onClick={this.handleReload} className="w-full">
                Reload
              </Button>
            </CardContent>
          </Card>
        </div>
      );
    }

    return this.props.children;
  }
}
