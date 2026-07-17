import React from "react";
import { Text, View } from "react-native";

import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import type { MindMapNodeDto } from "../../api/notes";

export interface MindMapOutlineProps {
  node: MindMapNodeDto;
  depth?: number;
  className?: string;
}

const INDENT_PER_DEPTH = 18;

/**
 * Renders the backend's nested `{topic, children}` mind map as an indented
 * outline — deliberately NOT a node-graph canvas (explicitly out of scope
 * for this phase). Each depth level indents further right via recursion; the
 * root gets a filled bullet and every deeper level a hollow one, so depth
 * reads clearly even before you consciously notice the indentation.
 */
export function MindMapOutline({ node, depth = 0, className = "" }: MindMapOutlineProps) {
  const { colors } = useTheme();

  return (
    <View className={className}>
      <View className="mb-2.5 flex-row items-center" style={{ paddingLeft: depth * INDENT_PER_DEPTH }}>
        <Icon
          name={depth === 0 ? "ellipse" : "ellipse-outline"}
          size={depth === 0 ? 9 : 7}
          color={depth === 0 ? colors.brand : colors.textSecondary}
        />
        <Text
          className={[
            "ml-2.5 flex-1",
            depth === 0
              ? "text-subheading text-ink-primary dark:text-ink-primary-dark"
              : "text-body text-ink-primary dark:text-ink-primary-dark",
          ].join(" ")}
        >
          {node.topic}
        </Text>
      </View>
      {node.children.map((child, index) => (
        <MindMapOutline key={`${depth}-${index}-${child.topic}`} node={child} depth={depth + 1} />
      ))}
    </View>
  );
}
