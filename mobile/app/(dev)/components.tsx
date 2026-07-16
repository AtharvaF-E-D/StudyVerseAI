import React, { useState } from "react";
import { Alert, Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Button } from "../../src/components/Button";
import { Card } from "../../src/components/Card";
import { Badge, type BadgeVariant } from "../../src/components/Badge";
import { Chip } from "../../src/components/Chip";
import { Avatar } from "../../src/components/Avatar";
import { Divider } from "../../src/components/Divider";
import { ListItem } from "../../src/components/ListItem";
import { ProgressBar } from "../../src/components/ProgressBar";
import { Switch } from "../../src/components/Switch";
import { Skeleton } from "../../src/components/Skeleton";
import { EmptyState } from "../../src/components/EmptyState";
import { ErrorState } from "../../src/components/ErrorState";
import { Icon } from "../../src/components/Icon";
import { useToast } from "../../src/lib/toast";
import { useTheme } from "../../src/theme/ThemeProvider";

const badgeVariants: BadgeVariant[] = ["neutral", "brand", "success", "warning", "danger"];

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <View className="mb-8">
      <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">{title}</Text>
      {children}
    </View>
  );
}

interface DemoChip {
  id: string;
  label: string;
  selected: boolean;
}

/**
 * Manual/automated visual QA screen for every Phase 2 shared component.
 * Not linked from any navigation — reached directly at `/(dev)/components`.
 * Deliberately plain: this screen's only job is to prove every component
 * renders correctly (and visibly styled) in both light and dark mode, not
 * to look polished itself.
 */
export default function ComponentsShowcaseScreen() {
  const { scheme, setScheme, colors } = useTheme();
  const { show } = useToast();

  const [chips, setChips] = useState<DemoChip[]>([
    { id: "algebra", label: "Algebra", selected: true },
    { id: "biology", label: "Biology", selected: false },
    { id: "history", label: "History", selected: false },
    { id: "custom-tag", label: "My custom tag", selected: false },
  ]);
  const [isSwitchOn, setIsSwitchOn] = useState(false);

  function toggleChip(id: string) {
    setChips((prev) => prev.map((c) => (c.id === id ? { ...c, selected: !c.selected } : c)));
  }

  function dismissChip(id: string) {
    setChips((prev) => prev.filter((c) => c.id !== id));
  }

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Component Showcase</Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <Section title="Card">
        <Text className="mb-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">flat</Text>
        <Card elevation="flat" className="mb-4">
          <Text className="text-body text-ink-primary dark:text-ink-primary-dark">
            Flat card — bordered surface, no shadow.
          </Text>
        </Card>

        <Text className="mb-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">raised</Text>
        <Card elevation="raised" className="mb-4">
          <Text className="text-body text-ink-primary dark:text-ink-primary-dark">
            Raised card — shadow only, no border.
          </Text>
        </Card>

        <Text className="mb-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">
          glass (reserved for overlays/floating surfaces)
        </Text>
        <View className="h-40 items-center justify-center overflow-hidden rounded-xl bg-surface dark:bg-surface-dark">
          <View className="absolute -left-4 top-2 h-24 w-24 rounded-full bg-brand" />
          <View className="absolute -right-6 bottom-0 h-28 w-28 rounded-full bg-accent" />
          <Card elevation="glass" className="mx-6">
            <Text className="text-body font-medium text-ink-primary dark:text-ink-primary-dark">
              Glass card floating above content.
            </Text>
          </Card>
        </View>
      </Section>

      <Section title="Badge">
        <View className="flex-row flex-wrap gap-2">
          {badgeVariants.map((variant) => (
            <Badge key={variant} label={variant} variant={variant} />
          ))}
        </View>
      </Section>

      <Section title="Chip">
        <View className="flex-row flex-wrap gap-2">
          {chips.map((chip) => (
            <Chip
              key={chip.id}
              label={chip.label}
              selected={chip.selected}
              onPress={() => toggleChip(chip.id)}
              onDismiss={chip.id === "custom-tag" ? () => dismissChip(chip.id) : undefined}
            />
          ))}
        </View>
      </Section>

      <Section title="Avatar">
        <View className="flex-row items-center gap-4">
          <Avatar source={require("../../assets/icon.png")} name="Ada Lovelace" size="lg" />
          <Avatar name="Grace Hopper" size="lg" />
          <Avatar name="Bo" size="md" />
          <Avatar size="sm" />
        </View>
      </Section>

      <Section title="Divider">
        <Text className="mb-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">
          Full width by default:
        </Text>
        <Divider />
      </Section>

      <Section title="ListItem">
        <Card elevation="flat">
          <ListItem
            leading={<Icon name="book-outline" size={22} color={colors.brand} />}
            title="Calculus fundamentals"
            subtitle="12 of 20 lessons complete"
            trailing={<Icon name="chevron-forward" size={20} color={colors.textSecondary} />}
            onPress={() => Alert.alert("Pressed", "Calculus fundamentals")}
          />
          <Divider />
          <ListItem
            leading={<Avatar name="Study Group" size="sm" />}
            title="Weekly study group"
            subtitle="Next session Thursday, 6pm"
            trailing={<Badge label="new" variant="brand" />}
            onPress={() => Alert.alert("Pressed", "Weekly study group")}
          />
          <Divider />
          <ListItem
            leading={<Icon name="notifications-outline" size={22} color={colors.textSecondary} />}
            title="Notifications"
            subtitle="Non-interactive row (no onPress)"
          />
        </Card>
      </Section>

      <Section title="ProgressBar">
        <Text className="mb-1 text-caption text-ink-secondary dark:text-ink-secondary-dark">25%</Text>
        <ProgressBar value={0.25} className="mb-4" accessibilityLabel="Example progress at 25 percent" />
        <Text className="mb-1 text-caption text-ink-secondary dark:text-ink-secondary-dark">60%</Text>
        <ProgressBar value={0.6} className="mb-4" accessibilityLabel="Example progress at 60 percent" />
        <Text className="mb-1 text-caption text-ink-secondary dark:text-ink-secondary-dark">100%</Text>
        <ProgressBar value={1} accessibilityLabel="Example progress at 100 percent" />
      </Section>

      <Section title="Switch">
        <View className="flex-row items-center justify-between">
          <Text className="text-body text-ink-primary dark:text-ink-primary-dark">Daily reminders</Text>
          <Switch
            value={isSwitchOn}
            onValueChange={setIsSwitchOn}
            accessibilityLabel="Toggle daily reminders"
          />
        </View>
      </Section>

      <Section title="Toast">
        <View className="mb-2">
          <Button title="Show success toast" onPress={() => show("Progress saved!", "success")} />
        </View>
        <Button
          title="Show danger toast"
          variant="secondary"
          onPress={() => show("Couldn't save your progress.", "danger")}
        />
      </Section>

      <Section title="Skeleton">
        <Skeleton variant="text" width="80%" className="mb-2" />
        <Skeleton variant="text" width="55%" className="mb-4" />
        <View className="flex-row items-center gap-3">
          <Skeleton variant="circle" />
          <Skeleton variant="rect" width={120} height={48} />
        </View>
      </Section>

      <Section title="EmptyState">
        <Card elevation="flat">
          <EmptyState
            icon="albums-outline"
            title="No flashcard decks yet"
            description="Create your first deck to start studying."
            actionLabel="Create deck"
            onAction={() => Alert.alert("Action", "Create deck pressed")}
          />
        </Card>
      </Section>

      <Section title="ErrorState">
        <Card elevation="flat">
          <ErrorState
            title="Couldn't load your quizzes"
            description="Check your connection and try again."
            onRetry={() => Alert.alert("Retry", "Retry pressed")}
          />
        </Card>
      </Section>
    </ScreenContainer>
  );
}
