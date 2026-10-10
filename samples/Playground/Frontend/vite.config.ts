import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import { existsSync } from "node:fs";
import { fileURLToPath } from "node:url";

const privatePreloader = fileURLToPath(
  new URL("./.licensed/preloader.tsx", import.meta.url),
);

export default defineConfig({
  plugins: [react(), tailwindcss({ optimize: false })],
  define: { "process.env.NODE_ENV": JSON.stringify("production") },
  resolve: {
    alias: {
      "@licensed/preloader":
        existsSync(privatePreloader) &&
        process.env.PLAYGROUND_PUBLIC_BUILD !== "1"
          ? privatePreloader
          : fileURLToPath(
              new URL("./src/components/loading-overlay.tsx", import.meta.url),
            ),
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  build: {
    outDir: "../wwwroot/ui",
    emptyOutDir: true,
    minify: "esbuild",
    cssMinify: "esbuild",
    lib: {
      entry: "src/main.tsx",
      formats: ["es"],
      fileName: () => "playground.js",
      cssFileName: "style",
    },
  },
});
