import { copyFile, mkdir, mkdtemp, rm, stat } from "node:fs/promises";
import { tmpdir } from "node:os";
import { basename, dirname, join, resolve } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";
import { build } from "esbuild";

const frontendRoot = fileURLToPath(new URL("../", import.meta.url));
const destination = resolve(frontendRoot, "../wwwroot/ui");
const stagingParent = resolve(tmpdir());
const stagingPrefix = "janglim-playground-build-";
const staging = await mkdtemp(join(stagingParent, stagingPrefix));

try {
  // Vite's synchronous output-directory cleanup crashes Node on this Windows
  // OneDrive checkout. Build in an owned temporary folder and copy only our
  // stable generated assets after the entire build succeeds.
  const result = spawnSync(
    process.execPath,
    [
      join(frontendRoot, "node_modules/vite/bin/vite.js"),
      "build",
      "--outDir",
      staging,
    ],
    {
      cwd: frontendRoot,
      stdio: "inherit",
      env: process.env,
      windowsHide: true,
    },
  );

  if (result.error) throw result.error;
  if (result.status !== 0) {
    if (result.signal) console.error(`Vite terminated by ${result.signal}.`);
    process.exitCode = result.status && result.status > 0 ? result.status : 1;
  } else {
    await build({
      entryPoints: [join(frontendRoot, "src/bootstrap.ts")],
      outfile: join(staging, "locale.js"),
      bundle: true,
      format: "iife",
      target: "es2022",
      minify: true,
    });
    const assets = ["playground.js", "style.css", "locale.js"];
    for (const asset of assets) {
      if (!(await stat(join(staging, asset))).isFile())
        throw new Error(`The build did not produce ${asset}.`);
    }
    await mkdir(destination, { recursive: true });
    for (const asset of assets)
      await copyFile(join(staging, asset), join(destination, asset));
    console.log(
      "Copied playground.js, style.css, and locale.js to wwwroot/ui.",
    );
  }
} catch (error) {
  console.error(error);
  process.exitCode = 1;
} finally {
  // Never recursively remove a path other than the unique directory created here.
  if (
    dirname(staging) !== stagingParent ||
    !basename(staging).startsWith(stagingPrefix)
  )
    throw new Error("Refusing to clean an unexpected build staging path.");
  try {
    await rm(staging, { recursive: true, force: true });
  } catch (error) {
    console.error("Unable to clean the build staging directory:", error);
    process.exitCode ||= 1;
  }
}
