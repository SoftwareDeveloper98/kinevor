import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  ApiEnvelope,
  DashboardAlerts,
  DashboardBranch,
  DashboardQuery,
  STAFF_DASHBOARD_API_BASE_URL,
  StaffDashboard,
  StaffDashboardGateway,
} from './staff-dashboard.model';

@Injectable()
export class HttpStaffDashboardGateway implements StaffDashboardGateway {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(STAFF_DASHBOARD_API_BASE_URL).replace(/\/$/, '');

  async get(query: DashboardQuery): Promise<StaffDashboard> {
    let params = new HttpParams().set('days', query.days ?? 7);

    if (query.branchId) params = params.set('branchId', query.branchId);

    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<StaffDashboard>>(`${this.baseUrl}/staff-dashboard`, { params }),
    );
    return response.data;
  }

  async alerts(branchId?: string | null): Promise<DashboardAlerts> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId);

    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<DashboardAlerts>>(`${this.baseUrl}/staff-dashboard/alerts`, {
        params,
      }),
    );
    return response.data;
  }

  async branches(): Promise<DashboardBranch[]> {
    const response = await firstValueFrom(
      this.http.get<ApiEnvelope<DashboardBranch[]>>(`${this.baseUrl}/staff-dashboard/branches`),
    );
    return response.data;
  }
}
