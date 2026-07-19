import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

import { parseVersion, updateUnityVersionText } from "./update-unity-version.mjs";

const PROJECT_SETTINGS_URL = new URL("../ProjectSettings/ProjectSettings.asset", import.meta.url);

test("SemVerをUnityのバージョン名と単調増加するビルド番号へ変換する", () => {
  assert.deepEqual(parseVersion("2.3.4"), {
    version: "2.3.4",
    major: 2,
    minor: 3,
    patch: 4,
    buildNumber: 2_003_004,
  });
});

test("ProjectSettingsの公開用バージョンを一括更新する", async () => {
  const source = await readFile(PROJECT_SETTINGS_URL, "utf8");
  const updated = updateUnityVersionText(source, "2.3.4");

  assert.match(updated, /^  bundleVersion: 2\.3\.4$/m);
  assert.match(updated, /^  visionOSBundleVersion: 2\.3\.4$/m);
  assert.match(updated, /^  tvOSBundleVersion: 2\.3\.4$/m);
  assert.match(updated, /^    Standalone: 2003004$/m);
  assert.match(updated, /^    VisionOS: 2003004$/m);
  assert.match(updated, /^    iPhone: 2003004$/m);
  assert.match(updated, /^    tvOS: 2003004$/m);
  assert.match(updated, /^  AndroidBundleVersionCode: 2003004$/m);
  assert.match(updated, /^  switchDisplayVersion: 2\.3\.4$/m);
  assert.match(updated, /^  XboxOneVersion: 2\.3\.4\.0$/m);
});

test("不正なバージョンと想定外のProjectSettingsを拒否する", () => {
  assert.throws(() => parseVersion("v2.3.4"), /major\.minor\.patch/);
  assert.throws(() => parseVersion("2100.0.1"), /Android/);
  assert.throws(() => updateUnityVersionText("bundleVersion: 1.0", "2.3.4"), /visionOSBundleVersion/);
});
