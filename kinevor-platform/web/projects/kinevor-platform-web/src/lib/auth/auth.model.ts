/**
 * The authenticated user as identified by the identity server.
 */
export interface AuthUser {
  /** Stable identifier of the user. */
  id: string;
  /** Display name of the user. */
  fullName: string;
  /** Email address used to sign in. */
  email: string;
  /** Role names assigned to the user, for example `Admin` or `Member`. */
  roles: string[];
}

/**
 * Result of a successful authentication or session restore.
 *
 * The payload differs between the login response, which carries a bearer
 * token, and the cookie-based session response used by the web client.
 */
export interface AuthSession {
  /** Authenticated user; present when the server returns a `user` object. */
  user?: AuthUser;
  /** User id for flat session payloads without a nested `user`. */
  id?: string;
  /** User display name for flat session payloads. */
  fullName?: string;
  /** User email for flat session payloads. */
  email?: string;
  /** Roles for flat session payloads. */
  roles?: string[];
  /** Short-lived access token; absent on cookie-based session restore. */
  accessToken?: string;
  /** ISO-8601 expiry timestamp used to schedule token refresh. */
  expiresAt?: string;
  /** Whether the login requires a second factor before it completes. */
  mfaRequired?: boolean;
}
