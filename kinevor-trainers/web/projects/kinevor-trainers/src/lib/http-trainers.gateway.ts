import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  ApiResponse,
  KINEVOR_API_BASE_URL,
  PagedResult,
  unwrapEnvelope,
} from '@softwaredeveloper98/kinevor-platform-web';
import { TrainerSummary } from './trainer.model';
import { TrainerListQuery, TrainersGateway } from './trainers.gateway';

/** HTTP implementation of {@link TrainersGateway} against `/trainers`. */
@Injectable()
export class HttpTrainersGateway implements TrainersGateway {
  private readonly http = inject(HttpClient);
  private readonly api = inject(KINEVOR_API_BASE_URL);

  /** {@inheritDoc TrainersGateway.list} */
  list(query: TrainerListQuery): Observable<PagedResult<TrainerSummary>> {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 20);

    if (query.search) params = params.set('search', query.search);

    return this.http
      .get<ApiResponse<PagedResult<TrainerSummary>>>(`${this.api}/trainers`, { params })
      .pipe(map(unwrapEnvelope));
  }

  /** {@inheritDoc TrainersGateway.byId} */
  byId(id: string): Observable<TrainerSummary> {
    return this.http
      .get<ApiResponse<TrainerSummary>>(`${this.api}/trainers/${id}`)
      .pipe(map(unwrapEnvelope));
  }
}
