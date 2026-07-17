import React, { useMemo, useState } from "react";
import { View } from "react-native";
import { WebView, type WebViewMessageEvent } from "react-native-webview";

import { KATEX_CSS, KATEX_JS } from "./katexAssets.generated";
import type { MathRendererProps, MathSegmentInput } from "./mathRendererTypes";

// ---------------------------------------------------------------------------
// Native (iOS/Android) math renderer. Loads a fully offline, locally bundled
// KaTeX (see `katexAssets.generated.ts` — the JS/CSS text embedded there was
// produced by copying `node_modules/katex/dist/*` and inlining its fonts as
// base64 so nothing is fetched from a CDN or a relative asset path the
// WebView's inline `source={{ html }}` load has no base URL for) inside a
// single WebView per message. All of a message's math segments (inline and
// block alike) render together in this one WebView; the surrounding plain
// text and code fences render as ordinary native components around it (see
// `MessageContent.tsx`) — a single native WebView is one rectangular view in
// the tree, so it can't literally interleave itself between separate Text
// nodes line-by-line, but grouping every math segment for a message into one
// instance keeps the "one WebView per message" contract from the spec and
// avoids mounting a costly WebView per individual formula.
//
// Auto-sizing: the HTML posts its rendered `document.body.scrollHeight` back
// via `window.ReactNativeWebView.postMessage`, and `onMessage` below resizes
// the WebView's own height to match — the standard bridge pattern for
// content-sized WebViews in RN, since a WebView has no intrinsic size of its
// own otherwise.
// ---------------------------------------------------------------------------

const MIN_HEIGHT = 20;

function buildHtml(segments: MathSegmentInput[], textColor: string): string {
  const body = segments
    .map(
      (segment) =>
        `<div class="math-segment ${segment.displayMode ? "math-block" : "math-inline"}" id="seg-${segment.id}"></div>`,
    )
    .join("\n");

  const segmentsJson = JSON.stringify(segments).replace(/</g, "\\u003c");

  return `<!doctype html>
<html>
  <head>
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1" />
    <style>${KATEX_CSS}</style>
    <style>
      html, body { margin: 0; padding: 0; background: transparent; }
      body { color: ${textColor}; font-size: 16px; }
      .math-segment { padding: 3px 0; max-width: 100%; overflow-x: auto; }
      .math-block { padding: 10px 0; }
      .katex { color: ${textColor}; }
      .katex-error { color: #E5484D; font-family: monospace; font-size: 13px; white-space: pre-wrap; }
    </style>
  </head>
  <body>
    ${body}
    <script>${KATEX_JS}</script>
    <script>
      var segments = ${segmentsJson};
      segments.forEach(function (segment) {
        var el = document.getElementById("seg-" + segment.id);
        if (!el) return;
        try {
          katex.render(segment.tex, el, { throwOnError: false, displayMode: segment.displayMode });
        } catch (e) {
          el.textContent = segment.tex;
          el.className += " katex-error";
        }
      });
      function reportHeight() {
        var height = document.body.scrollHeight;
        if (window.ReactNativeWebView) {
          window.ReactNativeWebView.postMessage(JSON.stringify({ height: height }));
        }
      }
      reportHeight();
      window.addEventListener("load", reportHeight);
      setTimeout(reportHeight, 60);
      setTimeout(reportHeight, 300);
    </script>
  </body>
</html>`;
}

export function MathRenderer({ segments, textColor }: MathRendererProps) {
  const [height, setHeight] = useState(MIN_HEIGHT);
  const html = useMemo(() => buildHtml(segments, textColor), [segments, textColor]);

  function handleMessage(event: WebViewMessageEvent) {
    try {
      const payload = JSON.parse(event.nativeEvent.data) as { height?: number };
      if (typeof payload.height === "number" && payload.height > 0) {
        setHeight(Math.ceil(payload.height));
      }
    } catch {
      // Ignore malformed/unexpected bridge messages.
    }
  }

  if (segments.length === 0) return null;

  return (
    <View style={{ height, width: "100%" }}>
      <WebView
        originWhitelist={["*"]}
        source={{ html }}
        onMessage={handleMessage}
        scrollEnabled={false}
        showsVerticalScrollIndicator={false}
        showsHorizontalScrollIndicator={false}
        style={{ backgroundColor: "transparent" }}
      />
    </View>
  );
}
