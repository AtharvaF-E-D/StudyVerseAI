import { apiClient } from "./client";

// ---------------------------------------------------------------------------
// Shared shapes
// ---------------------------------------------------------------------------

export interface AuthUserDto {
  id: string;
  email: string;
  displayName: string;
  emailVerified: boolean;
}

/** Shared response shape returned by login, otp/verify, google, and apple. */
export interface AuthSessionResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  user: AuthUserDto;
}

export type OtpChannel = "email" | "phone";

/**
 * Purpose tags the backend uses to scope an OTP to a particular flow, e.g.
 * verifying a freshly-registered email address vs. a passwordless login.
 */
export type OtpPurpose = "emailVerification" | "login";

// ---------------------------------------------------------------------------
// POST /register
// ---------------------------------------------------------------------------

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  message: string;
}

export async function register(payload: RegisterRequest): Promise<RegisterResponse> {
  const { data } = await apiClient.post<RegisterResponse>("/register", payload);
  return data;
}

// ---------------------------------------------------------------------------
// POST /login
// ---------------------------------------------------------------------------

export interface LoginRequest {
  email: string;
  password: string;
  deviceId: string;
  deviceName: string;
}

export async function login(payload: LoginRequest): Promise<AuthSessionResponse> {
  const { data } = await apiClient.post<AuthSessionResponse>("/login", payload);
  return data;
}

// ---------------------------------------------------------------------------
// POST /refresh
// ---------------------------------------------------------------------------

export interface RefreshRequest {
  refreshToken: string;
  deviceId: string;
}

export interface RefreshResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
}

export async function refreshSession(payload: RefreshRequest): Promise<RefreshResponse> {
  const { data } = await apiClient.post<RefreshResponse>("/refresh", payload);
  return data;
}

// ---------------------------------------------------------------------------
// POST /logout
// ---------------------------------------------------------------------------

export interface LogoutRequest {
  refreshToken: string;
  deviceId: string;
}

export async function logout(payload: LogoutRequest): Promise<void> {
  await apiClient.post<void>("/logout", payload);
}

// ---------------------------------------------------------------------------
// POST /verify-email
// ---------------------------------------------------------------------------

export interface VerifyEmailRequest {
  userId: string;
  token: string;
}

export async function verifyEmail(payload: VerifyEmailRequest): Promise<void> {
  await apiClient.post<void>("/verify-email", payload);
}

// ---------------------------------------------------------------------------
// POST /resend-verification
// ---------------------------------------------------------------------------

export interface ResendVerificationRequest {
  email: string;
}

export async function resendVerification(payload: ResendVerificationRequest): Promise<void> {
  await apiClient.post<void>("/resend-verification", payload);
}

// ---------------------------------------------------------------------------
// POST /forgot-password
// ---------------------------------------------------------------------------

export interface ForgotPasswordRequest {
  email: string;
}

export async function forgotPassword(payload: ForgotPasswordRequest): Promise<void> {
  await apiClient.post<void>("/forgot-password", payload);
}

// ---------------------------------------------------------------------------
// POST /reset-password
// ---------------------------------------------------------------------------

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

export async function resetPassword(payload: ResetPasswordRequest): Promise<void> {
  await apiClient.post<void>("/reset-password", payload);
}

// ---------------------------------------------------------------------------
// POST /otp/request
// ---------------------------------------------------------------------------

export interface OtpRequestRequest {
  channel: OtpChannel;
  destination: string;
  purpose: OtpPurpose;
}

export async function requestOtp(payload: OtpRequestRequest): Promise<void> {
  await apiClient.post<void>("/otp/request", payload);
}

// ---------------------------------------------------------------------------
// POST /otp/verify
// ---------------------------------------------------------------------------

export interface OtpVerifyRequest {
  channel: OtpChannel;
  destination: string;
  code: string;
  purpose: OtpPurpose;
  deviceId: string;
  deviceName: string;
}

export async function verifyOtp(payload: OtpVerifyRequest): Promise<AuthSessionResponse> {
  const { data } = await apiClient.post<AuthSessionResponse>("/otp/verify", payload);
  return data;
}

// ---------------------------------------------------------------------------
// POST /google
// ---------------------------------------------------------------------------

export interface GoogleSignInRequest {
  idToken: string;
  deviceId: string;
  deviceName: string;
}

export async function signInWithGoogle(payload: GoogleSignInRequest): Promise<AuthSessionResponse> {
  const { data } = await apiClient.post<AuthSessionResponse>("/google", payload);
  return data;
}

// ---------------------------------------------------------------------------
// POST /apple
// ---------------------------------------------------------------------------

export interface AppleSignInRequest {
  identityToken: string;
  deviceId: string;
  deviceName: string;
  fullName?: string;
}

export async function signInWithApple(payload: AppleSignInRequest): Promise<AuthSessionResponse> {
  const { data } = await apiClient.post<AuthSessionResponse>("/apple", payload);
  return data;
}

// ---------------------------------------------------------------------------
// GET /me
// ---------------------------------------------------------------------------

export interface MeResponse {
  id: string;
  email: string;
  displayName: string;
  emailVerified: boolean;
  createdAtUtc: string;
}

export async function getMe(): Promise<MeResponse> {
  const { data } = await apiClient.get<MeResponse>("/me");
  return data;
}
