import React from "react";
import { View } from "react-native";

import { Card } from "../Card";
import { Skeleton } from "../Skeleton";

function SkeletonListRow({ withAvatar = false }: { withAvatar?: boolean }) {
  return (
    <View className="flex-row items-center px-3 py-3">
      <Skeleton variant={withAvatar ? "circle" : "rect"} width={withAvatar ? undefined : 22} height={22} className="mr-3" />
      <View className="flex-1">
        <Skeleton variant="text" width="60%" className="mb-2" />
        <Skeleton variant="text" width="85%" />
      </View>
    </View>
  );
}

/**
 * Loading placeholder shown while the dashboard query is in flight, roughly
 * matching `DashboardContent`'s section layout so the screen doesn't jump
 * around once real data arrives.
 */
export function DashboardSkeleton() {
  return (
    <View>
      <View className="mb-6 flex-row items-center justify-between">
        <Skeleton variant="text" width="65%" height={28} />
        <Skeleton variant="circle" width={40} height={40} />
      </View>

      <Card className="mb-6">
        <View className="flex-row items-center justify-between">
          {[0, 1, 2].map((i) => (
            <View key={i} className="flex-1 items-center">
              <Skeleton variant="circle" width={26} height={26} className="mb-2" />
              <Skeleton variant="text" width={40} className="mb-1" />
              <Skeleton variant="text" width={56} />
            </View>
          ))}
        </View>
      </Card>

      <View className="mb-6">
        <Skeleton variant="text" width="45%" className="mb-3" />
        <Card>
          {[0, 1, 2].map((i) => (
            <SkeletonListRow key={i} />
          ))}
        </Card>
      </View>

      <View className="mb-6">
        <Skeleton variant="text" width="30%" className="mb-3" />
        <Card>
          <View className="flex-row items-end justify-between" style={{ height: 56 }}>
            {[0, 1, 2, 3, 4, 5, 6].map((i) => (
              <Skeleton key={i} variant="rect" width={18} height={16 + (i % 4) * 10} />
            ))}
          </View>
        </Card>
      </View>

      <View className="mb-6">
        <Skeleton variant="text" width="40%" className="mb-3" />
        <Card>
          {[0, 1, 2].map((i) => (
            <SkeletonListRow key={i} withAvatar />
          ))}
        </Card>
      </View>

      <View>
        <Skeleton variant="text" width="45%" className="mb-3" />
        <Card>
          {[0, 1].map((i) => (
            <SkeletonListRow key={i} />
          ))}
        </Card>
      </View>
    </View>
  );
}
