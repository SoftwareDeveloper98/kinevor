import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResult } from '@softwaredeveloper98/kinevor-platform-web';
import { TrainerSummary } from './trainer.model';

/** Server-side query parameters of the trainer directory. */
export interface TrainerListQuery {
  /** One-based page number. */
  page?: number;
  /** Page size. */
  pageSize?: number;
  /** Free-text search term matched against name and e-mail. */
  search?: string;
}

/** Data access for the trainer directory. */
export interface TrainersGateway {
  /**
   * Loads one page of trainers.
   *
   * @param query - Paging and search filters applied by the server.
   * @returns The requested page of trainers.
   */
  list(query: TrainerListQuery): Observable<PagedResult<TrainerSummary>>;

  /**
   * Loads a single trainer profile.
   *
   * @param id - Trainer identifier.
   * @returns The trainer record.
   */
  byId(id: string): Observable<TrainerSummary>;
}

/** Injection token resolving to the trainers data access implementation. */
export const TRAINERS_GATEWAY = new InjectionToken<TrainersGateway>('TrainersGateway');
