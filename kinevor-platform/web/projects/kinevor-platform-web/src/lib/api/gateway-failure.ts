/**
 * Normalized failure raised by a gateway when the API reports an
 * unsuccessful envelope or a payload that cannot be mapped.
 *
 * The shape mirrors problem details so that `I18nService.errorText` can
 * translate `error.code` the same way it translates HTTP failures.
 */
export class GatewayFailure extends Error {
  /** Problem-details view consumed by the translation layer. */
  readonly error: { code?: string; message?: string };

  /**
   * Creates a normalized gateway failure.
   *
   * @param code - Stable machine code, for example `MEMBER_NOT_FOUND`.
   * @param message - Fallback detail when the server sent no message.
   */
  constructor(code: string, message: string) {
    super(message);
    this.name = 'GatewayFailure';
    this.error = { code, message };
  }
}
