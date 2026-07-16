import React from "react";
import Ionicons from "@expo/vector-icons/Ionicons";
import { cssInterop } from "nativewind";

// Standardized on Ionicons app-wide (closest match to Material 3's rounded,
// geometric iconography while still shipping in @expo/vector-icons with no
// extra install). Every screen/component should import `Icon` from here
// instead of reaching into `@expo/vector-icons` directly, so swapping icon
// sets later is a one-file change.

// Ionicons renders a custom native Text component from inside
// `node_modules`, so it was never compiled through NativeWind's JSX
// transform the way this app's own `<Text>` usages are — without this
// explicit `cssInterop` call, a `className` passed to it is silently
// dropped (the exact class of bug already hit once in `Button.tsx`).
cssInterop(Ionicons, { className: "style" });

export type IconName = React.ComponentProps<typeof Ionicons>["name"];

export interface IconProps {
  name: IconName;
  size?: number;
  color?: string;
  className?: string;
}

export function Icon({ name, size = 24, color, className }: IconProps) {
  return <Ionicons name={name} size={size} color={color} className={className} />;
}
