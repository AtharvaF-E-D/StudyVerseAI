// `react-native-syntax-highlighter` (and the `react-syntax-highlighter`
// style modules it re-exposes) ships no TypeScript declarations and has no
// `@types/*` package on npm, so this ambient module declaration is the
// "add a new declaration (.d.ts) file" option TS itself suggests (TS7016)
// for an untyped JS dependency. Kept intentionally minimal — just the shape
// `CodeBlock.tsx` actually uses.
declare module "react-native-syntax-highlighter" {
  import type { ComponentType, ReactNode } from "react";

  export interface SyntaxHighlighterProps {
    language?: string;
    style?: unknown;
    highlighter?: "hljs" | "prism";
    fontFamily?: string;
    fontSize?: number;
    customStyle?: Record<string, unknown>;
    PreTag?: ComponentType<Record<string, unknown>>;
    CodeTag?: ComponentType<Record<string, unknown>>;
    children?: ReactNode;
  }

  const SyntaxHighlighter: ComponentType<SyntaxHighlighterProps>;
  export default SyntaxHighlighter;
}

declare module "react-syntax-highlighter/styles/hljs" {
  export const docco: unknown;
  export const atomOneDark: unknown;
}
