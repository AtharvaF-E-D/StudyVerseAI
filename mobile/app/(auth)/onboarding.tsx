import React, { useRef, useState } from "react";
import {
  Dimensions,
  Text,
  View,
  type NativeScrollEvent,
  type NativeSyntheticEvent,
  type ScrollView,
} from "react-native";
import { router } from "expo-router";
import Animated, {
  Extrapolation,
  interpolate,
  useAnimatedScrollHandler,
  useAnimatedStyle,
  useSharedValue,
  type SharedValue,
} from "react-native-reanimated";

import { Button } from "../../src/components/Button";
import { appStorage } from "../../src/lib/storage";

const { width: SCREEN_WIDTH } = Dimensions.get("window");
const ONBOARDING_SEEN_KEY = "studyverse.hasSeenOnboarding";

interface Slide {
  title: string;
  description: string;
}

const slides: Slide[] = [
  {
    title: "Learn smarter, not harder",
    description: "AI-guided study plans that adapt to how you actually learn.",
  },
  {
    title: "Track every subject",
    description: "See your progress across courses, topics, and deadlines in one place.",
  },
  {
    title: "Get instant help",
    description: "Ask questions and get clear, step-by-step explanations whenever you're stuck.",
  },
];

function OnboardingDot({ index, scrollX }: { index: number; scrollX: SharedValue<number> }) {
  const dotStyle = useAnimatedStyle(() => {
    const inputRange = [(index - 1) * SCREEN_WIDTH, index * SCREEN_WIDTH, (index + 1) * SCREEN_WIDTH];
    const width = interpolate(scrollX.value, inputRange, [8, 24, 8], Extrapolation.CLAMP);
    const opacity = interpolate(scrollX.value, inputRange, [0.3, 1, 0.3], Extrapolation.CLAMP);
    return { width, opacity };
  });

  return <Animated.View style={dotStyle} className="h-2 rounded-full bg-brand" />;
}

/**
 * 3-slide swipeable onboarding carousel. Marks onboarding as seen (in
 * MMKV) once the user finishes or skips, so `splash.tsx` routes straight to
 * login on subsequent launches.
 */
export default function OnboardingScreen() {
  const scrollX = useSharedValue(0);
  const [activeIndex, setActiveIndex] = useState(0);
  const scrollRef = useRef<ScrollView>(null);

  const scrollHandler = useAnimatedScrollHandler({
    onScroll: (event) => {
      scrollX.value = event.contentOffset.x;
    },
  });

  function handleMomentumScrollEnd(event: NativeSyntheticEvent<NativeScrollEvent>) {
    const index = Math.round(event.nativeEvent.contentOffset.x / SCREEN_WIDTH);
    setActiveIndex(index);
  }

  function handleGetStarted() {
    appStorage.setBoolean(ONBOARDING_SEEN_KEY, true);
    router.replace("/(auth)/login");
  }

  function handleNext() {
    if (activeIndex < slides.length - 1) {
      scrollRef.current?.scrollTo({ x: (activeIndex + 1) * SCREEN_WIDTH, animated: true });
    } else {
      handleGetStarted();
    }
  }

  const isLastSlide = activeIndex === slides.length - 1;

  return (
    <View className="flex-1 bg-background dark:bg-background-dark">
      <Animated.ScrollView
        ref={scrollRef}
        horizontal
        pagingEnabled
        showsHorizontalScrollIndicator={false}
        onScroll={scrollHandler}
        onMomentumScrollEnd={handleMomentumScrollEnd}
        scrollEventThrottle={16}
        className="flex-1"
      >
        {slides.map((slide) => (
          <View
            key={slide.title}
            style={{ width: SCREEN_WIDTH }}
            className="flex-1 items-center justify-center px-8"
          >
            <View className="mb-10 h-40 w-40 rounded-full bg-brand/10" />
            <Text className="mb-3 text-center text-heading text-ink-primary dark:text-ink-primary-dark">
              {slide.title}
            </Text>
            <Text className="text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
              {slide.description}
            </Text>
          </View>
        ))}
      </Animated.ScrollView>

      <View className="flex-row justify-center gap-2 pb-6">
        {slides.map((slide, index) => (
          <OnboardingDot key={slide.title} index={index} scrollX={scrollX} />
        ))}
      </View>

      <View className="px-6 pb-8">
        <Button title={isLastSlide ? "Get Started" : "Next"} onPress={handleNext} />
        {!isLastSlide ? (
          <Button
            title="Skip"
            variant="ghost"
            onPress={handleGetStarted}
            style={{ marginTop: 8 }}
          />
        ) : null}
      </View>
    </View>
  );
}
