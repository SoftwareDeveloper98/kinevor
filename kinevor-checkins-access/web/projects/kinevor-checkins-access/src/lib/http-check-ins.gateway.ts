import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  AccessAudit,
  AccessMemberSummary,
  ApiEnvelope,
  Branch,
  CheckInHistoryQuery,
  CHECKINS_API_BASE_URL,
  CheckInVisit,
  CheckInsGateway,
  CreateCheckInRequest,
  DailyStats,
  Eligibility,
  MemberCheckInStatus,
  MemberListQuery,
  Occupancy,
  PagedResult,
} from './check-in.model';

/** Typed HTTP gateway for the check-ins and access API. */
@Injectable()
export class HttpCheckInsGateway implements CheckInsGateway {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(CHECKINS_API_BASE_URL).replace(/\/$/, '');

  /** {@inheritDoc CheckInsGateway.members} */
  async members(query: MemberListQuery): Promise<PagedResult<AccessMemberSummary>> {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 20);

    if (query.search) params = params.set('search', query.search);

    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<PagedResult<AccessMemberSummary>>>(
        `${this.baseUrl}/checkins/members`,
        { params },
      ),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.eligibility} */
  async eligibility(memberId: string, branchId?: string | null): Promise<Eligibility> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId);

    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<Eligibility>>(
        `${this.baseUrl}/checkins/eligibility/${memberId}`,
        { params },
      ),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.history} */
  async history(query: CheckInHistoryQuery): Promise<PagedResult<CheckInVisit>> {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 25);

    if (query.search) params = params.set('search', query.search);
    if (query.memberId) params = params.set('memberId', query.memberId);
    if (query.branchId) params = params.set('branchId', query.branchId);
    if (query.from) params = params.set('from', query.from);
    if (query.to) params = params.set('to', query.to);
    if (query.status) params = params.set('status', query.status);

    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<PagedResult<CheckInVisit>>>(`${this.baseUrl}/checkins`, { params }),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.stats} */
  async stats(branchId?: string | null, date?: string | null): Promise<DailyStats> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId);
    if (date) params = params.set('date', date);

    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<DailyStats>>(`${this.baseUrl}/checkins/stats`, { params }),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.occupancy} */
  async occupancy(branchId?: string | null): Promise<Occupancy> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId);

    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<Occupancy>>(`${this.baseUrl}/checkins/occupancy`, { params }),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.branches} */
  async branches(): Promise<Branch[]> {
    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<Branch[]>>(`${this.baseUrl}/checkins/branches`),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.audit} */
  async audit(
    memberId?: string | null,
    page = 1,
    pageSize = 25,
  ): Promise<PagedResult<AccessAudit>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (memberId) params = params.set('memberId', memberId);

    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<PagedResult<AccessAudit>>>(`${this.baseUrl}/checkins/audit`, {
        params,
      }),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.checkIn} */
  async checkIn(request: CreateCheckInRequest): Promise<CheckInVisit> {
    const response = await firstValueFrom(
      this.http.post<ApiEnvelope<CheckInVisit>>(`${this.baseUrl}/checkins`, request),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.checkOut} */
  async checkOut(checkInId: string): Promise<CheckInVisit> {
    const response = await firstValueFrom(
      this.http.post<ApiEnvelope<CheckInVisit>>(
        `${this.baseUrl}/checkins/${checkInId}/checkout`,
        {},
      ),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.myStatus} */
  async myStatus(): Promise<MemberCheckInStatus> {
    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<MemberCheckInStatus>>(`${this.baseUrl}/me/checkins/status`),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.myVisits} */
  async myVisits(page = 1, pageSize = 20): Promise<PagedResult<CheckInVisit>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<PagedResult<CheckInVisit>>>(`${this.baseUrl}/me/checkins`, {
        params,
      }),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.selfCheckIn} */
  async selfCheckIn(notes?: string | null): Promise<CheckInVisit> {
    const response = await firstValueFrom(
      this.http.post<ApiEnvelope<CheckInVisit>>(`${this.baseUrl}/me/checkins`, {
        notes: notes ?? null,
      }),
    );
    return response.data;
  }

  /** {@inheritDoc CheckInsGateway.selfCheckOut} */
  async selfCheckOut(): Promise<CheckInVisit> {
    const response = await firstValueFrom(
      this.http.post<ApiEnvelope<CheckInVisit>>(`${this.baseUrl}/me/checkins/checkout`, {}),
    );
    return response.data;
  }
}
