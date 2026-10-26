/**
 * Success envelope returned by every `GET`/`POST` of the Kinevor API.
 *
 * @typeParam T - Type of the `data` payload.
 */
export interface ApiResponse<T> {
  /** Whether the request completed successfully. */
  success: boolean;
  /** Payload of a successful response. */
  data?: T;
  /** Human-readable message; may be localized by the server. */
  message?: string;
  /** Field-level validation errors, when any. */
  errors?: string[];
}

import { GatewayFailure } from './gateway-failure';

/**
 * Returns the payload of a successful envelope.
 *
 * @param response - Envelope received from the API.
 * @returns The `data` payload.
 * @throws {GatewayFailure} When the envelope reports failure or carries no payload.
 */
export function unwrapEnvelope<T>(response: ApiResponse<T>): T {
  if (!response.success) {
    throw new GatewayFailure('REQUEST_FAILED', response.message ?? 'Request failed');
  }
  if (response.data === undefined || response.data === null) {
    throw new GatewayFailure('EMPTY_RESPONSE', response.message ?? 'Response carried no payload');
  }
  return response.data;
}

/**
 * Asserts that the API accepted a request that returns no payload.
 *
 * @param response - Envelope received from the API.
 * @throws {GatewayFailure} When the envelope reports failure.
 */
export function unwrapOk(response: ApiResponse<unknown>): void {
  if (!response.success) {
    throw new GatewayFailure('REQUEST_FAILED', response.message ?? 'Request failed');
  }
}

/**
 * RFC 7807 problem details returned for failed requests.
 *
 * `code` is the stable machine code clients key off for translation.
 */
export interface ProblemDetails {
  /** Problem type URI. */
  type?: string;
  /** Short summary of the problem. */
  title?: string;
  /** HTTP status code. */
  status?: number;
  /** Human-readable explanation specific to this occurrence. */
  detail?: string;
  /** Stable machine code, for example `MEMBER_NOT_FOUND`. */
  code?: string;
  /** Correlation id for support requests. */
  traceId?: string;
  /** Field-level validation errors, when any. */
  errors?: string[];
}
