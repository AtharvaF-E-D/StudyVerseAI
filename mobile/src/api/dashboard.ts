import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// Shared shapes
// ---------------------------------------------------------------------------

export interface StreakDto {
  currentDays: number;
  longestDays: number;
  studiedToday: boolean;
}

export interface ChallengeDto {
  id: string;
  title: string;
  description: string;
  xpReward: number;
  coinReward: number;
  isCompleted: boolean;
}

export interface WeeklyActivityDayDto {
  /** "yyyy-MM-dd", oldest first. */
  date: string;
  xpEarned: number;
}

export interface LeaderboardEntryDto {
  userId: string;
  displayName: string;
  xp: number;
  rank: number;
}

export interface DashboardLeaderboardDto {
  myRank: number;
  top: LeaderboardEntryDto[];
}

export interface NotificationDto {
  id: string;
  title: string;
  body: string;
  createdAtUtc: string;
  readAtUtc: string | null;
}

export interface DashboardNotificationsDto {
  unreadCount: number;
  recent: NotificationDto[];
}

// ---------------------------------------------------------------------------
// GET /dashboard
// ---------------------------------------------------------------------------

export interface DashboardResponse {
  xp: number;
  level: number;
  coins: number;
  streak: StreakDto;
  todaysChallenges: ChallengeDto[];
  weeklyActivity: WeeklyActivityDayDto[];
  leaderboard: DashboardLeaderboardDto;
  notifications: DashboardNotificationsDto;
}

export async function getDashboard(): Promise<DashboardResponse> {
  const { data } = await coreApiClient.get<DashboardResponse>("/dashboard");
  return data;
}

// ---------------------------------------------------------------------------
// POST /dashboard/challenges/{challengeId}/complete
// ---------------------------------------------------------------------------

export interface CompleteChallengeResponse {
  xpAwarded: number;
  coinsAwarded: number;
  newXpTotal: number;
  newCoinsTotal: number;
  newLevel: number;
}

export async function completeChallenge(challengeId: string): Promise<CompleteChallengeResponse> {
  const { data } = await coreApiClient.post<CompleteChallengeResponse>(
    `/dashboard/challenges/${challengeId}/complete`,
  );
  return data;
}

// ---------------------------------------------------------------------------
// GET /notifications
// ---------------------------------------------------------------------------

export interface GetNotificationsParams {
  onlyUnread?: boolean;
  take?: number;
}

export async function getNotifications(params?: GetNotificationsParams): Promise<NotificationDto[]> {
  const { data } = await coreApiClient.get<NotificationDto[]>("/notifications", { params });
  return data;
}

// ---------------------------------------------------------------------------
// POST /notifications/{id}/read
// ---------------------------------------------------------------------------

export async function markNotificationRead(id: string): Promise<void> {
  await coreApiClient.post<void>(`/notifications/${id}/read`);
}

// ---------------------------------------------------------------------------
// GET /leaderboard
// ---------------------------------------------------------------------------

export async function getLeaderboard(take?: number): Promise<LeaderboardEntryDto[]> {
  const { data } = await coreApiClient.get<LeaderboardEntryDto[]>("/leaderboard", {
    params: take !== undefined ? { take } : undefined,
  });
  return data;
}
