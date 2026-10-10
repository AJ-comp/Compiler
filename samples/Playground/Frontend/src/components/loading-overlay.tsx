import type { ReactNode } from "react";
import { LoaderCircle } from "lucide-react";

// Public fallback for builds without the developer's privately licensed animation.
export default function LoadingOverlay({
  loading,
  customContent,
  loadingText = "Parsing your grammar…",
  ariaLabel,
}: {
  loading: boolean;
  customContent?: (progress: number) => ReactNode;
  variant?: string;
  position?: string;
  duration?: number;
  respectReducedMotion?: boolean;
  bgColor?: string;
  loadingText?: string;
  ariaLabel?: string;
}) {
  if (!loading) return null;
  return (
    <div
      className="loading-overlay"
      role="status"
      aria-label={ariaLabel ?? loadingText}
    >
      <LoaderCircle className="spin" size={24} />
      {customContent?.(0) ?? loadingText}
    </div>
  );
}
