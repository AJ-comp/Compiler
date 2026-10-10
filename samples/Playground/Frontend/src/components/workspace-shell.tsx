import type { ReactNode } from "react";

/**
 * Minimal workbench adaptation of Shadcnblocks' free Application Shell 1.
 * Source: https://www.shadcnblocks.com/block/application-shell1
 * The sidebar/main composition is retained; navigation and content are supplied
 * by Janglim rather than the block's demonstration data.
 * Free-block end-product terms: https://www.shadcnblocks.com/terms
 */
interface WorkspaceShellProps {
  rail: ReactNode;
  children: ReactNode;
}

export function WorkspaceShell({ rail, children }: WorkspaceShellProps) {
  return (
    <div className="app-shell" data-slot="workspace-shell">
      {rail}
      {children}
    </div>
  );
}
