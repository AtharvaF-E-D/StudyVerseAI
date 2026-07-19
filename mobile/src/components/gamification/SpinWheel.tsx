import React, { useEffect, useRef } from "react";
import { View } from "react-native";
import Animated, {
  cancelAnimation,
  Easing,
  runOnJS,
  useAnimatedStyle,
  useSharedValue,
  withRepeat,
  withTiming,
} from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { Icon, type IconName } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import { useReduceMotion } from "../../theme/motion";

cssInterop(Animated.View, { className: "style" });

// ---------------------------------------------------------------------------
// A true segmented pie-chart wheel (colored wedges + a conic gradient) isn't
// achievable with plain React Native `View`s — there's no conic-gradient
// primitive and this app has no SVG dependency to lean on (`react-native-svg`
// isn't installed; see this file's header note below for why one wasn't
// added just for this). So this takes the documented fallback the phase
// brief explicitly sanctions: a real circular wheel with real segments
// arranged in a ring (like a roulette wheel's prize pockets) that spin as one
// rigid group via a genuine Reanimated rotation, rather than a literal
// filled pie chart. It still delivers everything the brief asks for — a
// segmented circular wheel, a fast indefinite spin while the real request is
// in flight, and a decisive easing-out deceleration onto a specific segment
// once the response lands — just built from circles/icons instead of wedges.
//
// The 8 segments below are generic reward-flavor icons (gift/star/cash/...),
// not literal copies of the backend's freeform `prizeLabel` string — there's
// no way to know in advance which of the server's possible prize strings
// will come back, so pre-baking real prize text into fixed wedges would only
// be right by coincidence. Instead the wheel spins for suspense and lands on
// one of these generic segments; the actual `prizeLabel`/coins/xp the
// backend returned is then shown in an unambiguous reveal panel right below
// the wheel (see `SpinWheelCard.tsx`) — that panel, not the wheel face, is
// the authoritative "what did I win" readout.
// ---------------------------------------------------------------------------

interface SpinWheelSegment {
  iconName: IconName;
  colorKey: "brand" | "accent" | "warning" | "success";
}

const SEGMENTS: SpinWheelSegment[] = [
  { iconName: "gift", colorKey: "brand" },
  { iconName: "star", colorKey: "warning" },
  { iconName: "cash", colorKey: "success" },
  { iconName: "diamond", colorKey: "accent" },
  { iconName: "flash", colorKey: "brand" },
  { iconName: "trophy", colorKey: "warning" },
  { iconName: "sparkles", colorKey: "success" },
  { iconName: "ribbon", colorKey: "accent" },
];

export const SPIN_WHEEL_SEGMENT_COUNT = SEGMENTS.length;

const WHEEL_SIZE = 240;
const RING_RADIUS = 92;
const SEGMENT_SIZE = 48;
/** Extra full rotations added on top of the precise landing angle, purely for a longer, more satisfying spin. */
const LANDING_EXTRA_SPINS = 4;
const SUSPENSE_SPIN_DURATION_MS = 650;
const LANDING_DURATION_MS = 1400;

// Mirrors `Badge.tsx`'s variant class maps — same brand/accent/warning/success
// palette, same "no explicit dark: override for accent/warning/success"
// convention (those tokens only differ by scheme via `useTheme().colors`,
// not via a separate Tailwind dark shade).
const segmentContainerClasses: Record<SpinWheelSegment["colorKey"], string> = {
  brand: "bg-brand/15 border-brand dark:bg-brand-light/20 dark:border-brand-light",
  accent: "bg-accent/15 border-accent",
  warning: "bg-warning/15 border-warning",
  success: "bg-success/15 border-success",
};

export interface SpinWheelProps {
  /** True while a spin is in flight (fast, indefinite rotation) — i.e. from tap until `landingIndex` is set. */
  isSpinning: boolean;
  /** Set once the real `POST /spin` response has arrived; the wheel decelerates to a stop on this segment. `null` means "not landing yet". */
  landingIndex: number | null;
  /** Fires exactly once, when the landing deceleration finishes. */
  onLandingComplete?: () => void;
  className?: string;
}

export function SpinWheel({ isSpinning, landingIndex, onLandingComplete, className = "" }: SpinWheelProps) {
  const { colors } = useTheme();
  const reduceMotion = useReduceMotion();
  const rotation = useSharedValue(0);
  const hasLandedForCurrentSpinRef = useRef(false);

  // Suspense phase: spin fast and indefinitely while waiting on both the
  // real response and a `landingIndex`. A plain linear loop here (rather
  // than the design system's usual eased motion) mirrors the same
  // "continuous, mechanical while indeterminate" allowance `ActivityIndicator`
  // already gets — this loop IS the "still working" signal, same idea.
  useEffect(() => {
    if (isSpinning && landingIndex === null) {
      hasLandedForCurrentSpinRef.current = false;
      if (reduceMotion) return;
      rotation.value = withRepeat(
        withTiming(rotation.value + 360, { duration: SUSPENSE_SPIN_DURATION_MS, easing: Easing.linear }),
        -1,
        false,
      );
    }
  }, [isSpinning, landingIndex, reduceMotion, rotation]);

  // Landing phase: once the backend's picked a prize and the caller has
  // mapped it to a segment index, cancel the indefinite spin and ease-out
  // decelerate onto that exact segment — an "arriving under its own motion"
  // entrance per `theme/motion.ts`'s guidance, just applied to a rotation
  // instead of a translate/fade.
  useEffect(() => {
    if (landingIndex === null || hasLandedForCurrentSpinRef.current) return;
    hasLandedForCurrentSpinRef.current = true;

    const segmentAngle = 360 / SEGMENTS.length;
    const targetSegmentAngle = landingIndex * segmentAngle;
    const currentValue = rotation.value;
    const remainderIntoTurn = ((currentValue % 360) + 360) % 360;
    // Rotate however much more is needed (mod 360) so this segment ends up
    // at the top, under the fixed pointer, then add the extra full spins.
    const deltaToTarget = (((360 - targetSegmentAngle - remainderIntoTurn) % 360) + 360) % 360;
    const finalRotation = currentValue + LANDING_EXTRA_SPINS * 360 + deltaToTarget;

    cancelAnimation(rotation);

    if (reduceMotion) {
      // eslint-disable-next-line react-hooks/immutability
      rotation.value = finalRotation;
      onLandingComplete?.();
      return;
    }

    rotation.value = withTiming(
      finalRotation,
      { duration: LANDING_DURATION_MS, easing: Easing.out(Easing.cubic) },
      (finished) => {
        if (finished && onLandingComplete) runOnJS(onLandingComplete)();
      },
    );
  }, [landingIndex, reduceMotion, rotation, onLandingComplete]);

  const wheelStyle = useAnimatedStyle(() => ({ transform: [{ rotate: `${rotation.value}deg` }] }));

  const iconColorFor = (key: SpinWheelSegment["colorKey"]) =>
    key === "brand" ? colors.brand : key === "accent" ? colors.accent : key === "warning" ? colors.warning : colors.success;

  return (
    <View className={["items-center", className].join(" ")}>
      <View accessibilityElementsHidden importantForAccessibility="no-hide-descendants">
        <Icon name="caret-down" size={26} color={colors.brand} />
      </View>
      <View
        style={{ width: WHEEL_SIZE, height: WHEEL_SIZE, borderRadius: WHEEL_SIZE / 2, marginTop: -4 }}
        className="items-center justify-center border-4 border-border bg-surface dark:border-border-dark dark:bg-surface-dark"
      >
        <Animated.View style={[{ width: WHEEL_SIZE, height: WHEEL_SIZE }, wheelStyle]}>
          {SEGMENTS.map((segment, index) => {
            const angleRad = (index * (360 / SEGMENTS.length) * Math.PI) / 180;
            const left = WHEEL_SIZE / 2 + RING_RADIUS * Math.sin(angleRad) - SEGMENT_SIZE / 2;
            const top = WHEEL_SIZE / 2 - RING_RADIUS * Math.cos(angleRad) - SEGMENT_SIZE / 2;
            return (
              <View
                key={index}
                style={{ position: "absolute", left, top, width: SEGMENT_SIZE, height: SEGMENT_SIZE }}
                className={["items-center justify-center rounded-full border-2", segmentContainerClasses[segment.colorKey]].join(" ")}
              >
                <Icon name={segment.iconName} size={20} color={iconColorFor(segment.colorKey)} />
              </View>
            );
          })}
        </Animated.View>
        <View className="absolute h-5 w-5 rounded-full bg-brand dark:bg-brand-light" />
      </View>
    </View>
  );
}
