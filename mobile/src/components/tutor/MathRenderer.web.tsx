import React, { useEffect, useRef } from "react";
import { View } from "react-native";
import katex from "katex";

import { KATEX_CSS } from "./katexAssets.generated";
import type { MathRendererProps } from "./mathRendererTypes";

// ---------------------------------------------------------------------------
// Web variant (Metro picks this file automatically for `--web` builds via
// the `.web.tsx` platform extension). `react-native-webview` ships no web
// implementation at all — its generic fallback literally renders the text
// "React Native WebView does not support this platform" — so on web there is
// no WebView to render into. Since react-native-web's `View` is a real DOM
// node here, we skip the WebView entirely and render KaTeX straight into the
// DOM via the `katex` npm package's `render()` (imported directly — it's a
// browser-first library and bundles fine through Metro's web target),
// injecting the same offline, font-embedded CSS text used by the native
// WebView (see `katexAssets.generated.ts`) once per app session. This is
// arguably a better outcome than an iframe on web (no bridge/height
// round-trip needed — the DOM sizes itself), while staying true to the
// spec's actual goal: correct, offline, no-CDN KaTeX rendering.
// ---------------------------------------------------------------------------

let stylesInjected = false;

function ensureKatexStylesInjected() {
  if (stylesInjected || typeof document === "undefined") return;
  const style = document.createElement("style");
  style.setAttribute("data-katex-styles", "true");
  style.textContent = KATEX_CSS;
  document.head.appendChild(style);
  stylesInjected = true;
}

export function MathRenderer({ segments, textColor }: MathRendererProps) {
  const containerRef = useRef<View>(null);

  useEffect(() => {
    ensureKatexStylesInjected();
    const node = containerRef.current as unknown as HTMLElement | null;
    if (!node) return;

    node.innerHTML = "";
    for (const segment of segments) {
      const wrapper = document.createElement("div");
      wrapper.style.color = textColor;
      wrapper.style.overflowX = "auto";
      wrapper.style.maxWidth = "100%";
      wrapper.style.padding = segment.displayMode ? "8px 0" : "2px 0";
      try {
        katex.render(segment.tex, wrapper, { throwOnError: false, displayMode: segment.displayMode });
      } catch {
        wrapper.textContent = segment.tex;
      }
      node.appendChild(wrapper);
    }
  }, [segments, textColor]);

  if (segments.length === 0) return null;

  return <View ref={containerRef} />;
}
