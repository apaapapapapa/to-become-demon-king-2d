import { readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";

const PROJECT_SETTINGS_URL = new URL("../ProjectSettings/ProjectSettings.asset", import.meta.url);

export function parseVersion(version) {
  const match = /^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$/.exec(version);
  if (!match) {
    throw new Error(`バージョンはmajor.minor.patch形式で指定してください: ${version}`);
  }

  const major = Number(match[1]);
  const minor = Number(match[2]);
  const patch = Number(match[3]);
  if (minor > 999 || patch > 999) {
    throw new Error(`Unityのビルド番号へ変換できる範囲を超えています: ${version}`);
  }

  const buildNumber = major * 1_000_000 + minor * 1_000 + patch;
  if (buildNumber > 2_100_000_000) {
    throw new Error(`Androidのバージョンコード上限を超えています: ${version}`);
  }

  return {
    version,
    major,
    minor,
    patch,
    buildNumber,
  };
}

function replaceExactlyOnce(source, pattern, replacement, label) {
  const matches = source.match(new RegExp(pattern.source, pattern.flags.includes("g") ? pattern.flags : `${pattern.flags}g`));
  if (matches?.length !== 1) {
    throw new Error(`${label}は1箇所だけ存在する必要があります（検出数: ${matches?.length ?? 0}）`);
  }

  return source.replace(pattern, replacement);
}

export function updateUnityVersionText(source, nextVersion) {
  const version = parseVersion(nextVersion);
  const replacements = [
    [/^(  visionOSBundleVersion: ).*$/m, `$1${version.version}`, "visionOSBundleVersion"],
    [/^(  tvOSBundleVersion: ).*$/m, `$1${version.version}`, "tvOSBundleVersion"],
    [/^(  bundleVersion: ).*$/m, `$1${version.version}`, "bundleVersion"],
    [
      /^(  buildNumber:\r?\n)(    Standalone: ).*(\r?\n)(    VisionOS: ).*(\r?\n)(    iPhone: ).*(\r?\n)(    tvOS: ).*$/m,
      `$1$2${version.buildNumber}$3$4${version.buildNumber}$5$6${version.buildNumber}$7$8${version.buildNumber}`,
      "buildNumber",
    ],
    [/^(  AndroidBundleVersionCode: ).*$/m, `$1${version.buildNumber}`, "AndroidBundleVersionCode"],
    [/^(  switchDisplayVersion: ).*$/m, `$1${version.version}`, "switchDisplayVersion"],
    [/^(  XboxOneVersion: ).*$/m, `$1${version.version}.0`, "XboxOneVersion"],
  ];

  return replacements.reduce(
    (updated, [pattern, replacement, label]) => replaceExactlyOnce(updated, pattern, replacement, label),
    source,
  );
}

export async function updateUnityVersion(nextVersion, projectSettingsUrl = PROJECT_SETTINGS_URL) {
  const source = await readFile(projectSettingsUrl, "utf8");
  const updated = updateUnityVersionText(source, nextVersion);
  await writeFile(projectSettingsUrl, updated, "utf8");
}

const isDirectExecution = process.argv[1]
  && import.meta.url === pathToFileURL(resolve(process.argv[1])).href;

if (isDirectExecution) {
  const nextVersion = process.argv[2];
  if (!nextVersion) {
    throw new Error("更新するバージョンを指定してください。");
  }

  await updateUnityVersion(nextVersion);
  console.log(`Unity Playerのバージョンを${nextVersion}へ更新しました。`);
}
