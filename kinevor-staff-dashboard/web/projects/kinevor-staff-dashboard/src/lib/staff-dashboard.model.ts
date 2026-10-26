import { InjectionToken } from '@angular/core';

export type DashboardDays = 7 | 30;

export type AlertSeverity = 'Critical' | 'Warning' | 'Info';

export interface DashboardBranch {
  readonly id: string;
  readonly name: string;
  readonly city: string;
  readonly capacity: number;
  readonly activeMembers: number;
  readonly occupancyNow: number;
  readonly occupancyPercent: number;
}

export interface DashboardComparison {
  readonly activeMembersPercent: number;
  readonly revenuePercent: number;
}

export interface DashboardSummary {
  readonly totalMembers: number;
  readonly activeMembers: number;
  readonly occupancyNow: number;
  readonly occupancyPercent: number;
  readonly capacity: number;
  readonly checkInsToday: number;
  readonly bookingsToday: number;
  readonly revenueMonth: number;
  readonly outstandingInvoices: number;
  readonly outstandingAmount: number;
  readonly comparison: DashboardComparison;
}

export interface DashboardTrendPoint {
  readonly date: string;
  readonly label: string;
  readonly checkIns: number;
  readonly bookings: number;
  readonly revenue: number;
}

export interface DashboardActivity {
  readonly id: string;
  readonly branchId: string;
  readonly branchName: string;
  readonly branchCity: string;
  readonly kind: string;
  readonly title: string;
  readonly detail: string;
  readonly occurredAt: string;
}

export interface DashboardAlert {
  readonly id: string;
  readonly branchId: string;
  readonly branchName: string;
  readonly branchCity: string;
  readonly type: string;
  readonly severity: string;
  readonly title: string;
  readonly detail: string;
  readonly dueAt: string | null;
  readonly reference: string;
}

export interface StaffDashboard {
  readonly generatedAt: string;
  readonly branchId: string | null;
  readonly branchName: string;
  readonly branchCity: string;
  readonly days: number;
  readonly summary: DashboardSummary;
  readonly trend: DashboardTrendPoint[];
  readonly activity: DashboardActivity[];
  readonly alerts: DashboardAlert[];
  readonly branches: DashboardBranch[];
}

export interface DashboardAlerts {
  readonly generatedAt: string;
  readonly branchId: string | null;
  readonly branchName: string;
  readonly branchCity: string;
  readonly alerts: DashboardAlert[];
  readonly branches: DashboardBranch[];
}

export interface DashboardQuery {
  readonly branchId?: string | null;
  readonly days?: DashboardDays;
}

export interface ApiEnvelope<T> {
  readonly success: boolean;
  readonly data: T;
  readonly message?: string | null;
  readonly errors?: string[] | null;
}

export interface StaffDashboardGateway {
  get(query: DashboardQuery): Promise<StaffDashboard>;
  alerts(branchId?: string | null): Promise<DashboardAlerts>;
  branches(): Promise<DashboardBranch[]>;
}

export const STAFF_DASHBOARD_GATEWAY = new InjectionToken<StaffDashboardGateway>(
  'StaffDashboardGateway',
);

export const STAFF_DASHBOARD_API_BASE_URL = new InjectionToken<string>(
  'STAFF_DASHBOARD_API_BASE_URL',
);

export const DASHBOARD_RANGES: readonly DashboardDays[] = [7, 30];

export const ALERT_SEVERITIES: readonly AlertSeverity[] = ['Critical', 'Warning', 'Info'];

export const DASHBOARD_ROUTES_PATH = 'dashboard';
