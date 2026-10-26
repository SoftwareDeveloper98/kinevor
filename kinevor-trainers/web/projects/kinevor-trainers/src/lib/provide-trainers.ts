import { EnvironmentProviders, Provider, Type, makeEnvironmentProviders } from '@angular/core';
import { KINEVOR_API_BASE_URL } from '@softwaredeveloper98/kinevor-platform-web';
import { HttpTrainersGateway } from './http-trainers.gateway';
import { TRAINERS_GATEWAY, TrainersGateway } from './trainers.gateway';

/** Configuration accepted by {@link provideTrainers}. */
export interface TrainersOptions {
  /** Base URL of the trainers API, for example `https://api.example.com/api/v1`. */
  apiBaseUrl?: string;
  /** Alternative gateway implementation; defaults to {@link HttpTrainersGateway}. */
  gateway?: Type<TrainersGateway>;
}

/**
 * Registers the trainers feature providers.
 *
 * The host application must also provide `HttpClient` (for example with
 * `provideHttpClient`) unless a custom gateway that does not use HTTP is used.
 *
 * @param options - API base URL and gateway override.
 * @returns Providers for the trainers feature.
 */
export function provideTrainers(options: TrainersOptions = {}): EnvironmentProviders {
  const providers: (Provider | EnvironmentProviders)[] = [
    {
      provide: TRAINERS_GATEWAY,
      useClass: options.gateway ?? HttpTrainersGateway,
    },
  ];

  if (options.apiBaseUrl) {
    providers.push({ provide: KINEVOR_API_BASE_URL, useValue: options.apiBaseUrl });
  }

  return makeEnvironmentProviders(providers);
}
