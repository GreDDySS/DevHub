import React from "react";
import { Shell } from "@/components/layout/Shell";
import { ErrorBoundary } from "@/components/ErrorBoundary";

function App() {
  return (
    <ErrorBoundary>
      <Shell />
    </ErrorBoundary>
  );
}

export default App;
