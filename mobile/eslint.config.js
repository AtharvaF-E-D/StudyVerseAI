// https://docs.expo.dev/guides/using-eslint/
const { defineConfig } = require('eslint/config');
const expoConfig = require("eslint-config-expo/flat");

module.exports = defineConfig([
  expoConfig,
  {
    // `assets/katex/` holds an unmodified, provenance copy of KaTeX's built
    // dist files (see `src/components/tutor/katexAssets.generated.ts`) —
    // third-party minified/UMD code, not app source, and not meant to be
    // linted (or even executed directly; the app reads its text content at
    // build time only, via the generation script that produced the
    // `.generated.ts` module actually bundled into the app).
    ignores: ["dist/*", "assets/katex/**"],
  }
]);
