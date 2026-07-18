import React from "react";
import { ScrollView } from "react-native";

import { Chip } from "../Chip";

export interface CategoryChipRowProps {
  categories: string[];
  selectedCategory: string;
  onSelect: (category: string) => void;
  className?: string;
}

/** Capitalizes a lowercase wire category (e.g. `"world"`) for display (`"World"`). */
function categoryLabel(category: string): string {
  return category.length > 0 ? category[0]!.toUpperCase() + category.slice(1) : category;
}

/** Horizontally-scrolling row of category `Chip`s for the news feed's category selector. */
export function CategoryChipRow({ categories, selectedCategory, onSelect, className = "" }: CategoryChipRowProps) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerClassName="flex-row gap-2 pr-2"
      className={className}
    >
      {categories.map((category) => (
        <Chip
          key={category}
          label={categoryLabel(category)}
          selected={category === selectedCategory}
          onPress={() => onSelect(category)}
        />
      ))}
    </ScrollView>
  );
}
