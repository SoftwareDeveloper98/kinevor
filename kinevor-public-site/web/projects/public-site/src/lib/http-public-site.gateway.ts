import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import {
  ApiEnvelope,
  BlogPost,
  BlogPostSummary,
  Branch,
  ClassSession,
  Faq,
  Lead,
  LeadCreated,
  LeadQuery,
  LeadRequest,
  MembershipPlan,
  PagedResult,
  PublicSite,
  PUBLIC_SITE_API_BASE_URL,
  PublicSiteGateway,
  ScheduleQuery,
  Trainer,
  TrialCreated,
  TrialRequest,
} from './public-site.models';

@Injectable()
export class HttpPublicSiteGateway implements PublicSiteGateway {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(PUBLIC_SITE_API_BASE_URL).replace(/\/$/, '');

  async site(): Promise<PublicSite> {
    return this.unwrap(
      this.http.get<ApiEnvelope<PublicSite>>(`${this.baseUrl}/public/site`),
    );
  }

  async branches(): Promise<Branch[]> {
    return this.unwrap(
      this.http.get<ApiEnvelope<Branch[]>>(`${this.baseUrl}/public/branches`),
    );
  }

  async branch(slug: string): Promise<Branch> {
    return this.unwrap(
      this.http.get<ApiEnvelope<Branch>>(
        `${this.baseUrl}/public/branches/${encodeURIComponent(slug)}`,
      ),
    );
  }

  async plans(): Promise<MembershipPlan[]> {
    return this.unwrap(
      this.http.get<ApiEnvelope<MembershipPlan[]>>(`${this.baseUrl}/public/plans`),
    );
  }

  async trainers(): Promise<Trainer[]> {
    return this.unwrap(
      this.http.get<ApiEnvelope<Trainer[]>>(`${this.baseUrl}/public/trainers`),
    );
  }

  async schedule(query: ScheduleQuery = {}): Promise<ClassSession[]> {
    let params = new HttpParams();
    if (query.from) params = params.set('from', query.from);
    if (query.to) params = params.set('to', query.to);
    if (query.branchId) params = params.set('branchId', query.branchId);

    return this.unwrap(
      this.http.get<ApiEnvelope<ClassSession[]>>(`${this.baseUrl}/public/schedule`, {
        params,
      }),
    );
  }

  async faqs(): Promise<Faq[]> {
    return this.unwrap(
      this.http.get<ApiEnvelope<Faq[]>>(`${this.baseUrl}/public/faqs`),
    );
  }

  async posts(): Promise<BlogPostSummary[]> {
    return this.unwrap(
      this.http.get<ApiEnvelope<BlogPostSummary[]>>(`${this.baseUrl}/public/posts`),
    );
  }

  async post(slug: string): Promise<BlogPost> {
    return this.unwrap(
      this.http.get<ApiEnvelope<BlogPost>>(
        `${this.baseUrl}/public/posts/${encodeURIComponent(slug)}`,
      ),
    );
  }

  async createTrial(request: TrialRequest): Promise<TrialCreated> {
    return this.unwrap(
      this.http.post<ApiEnvelope<TrialCreated>>(`${this.baseUrl}/public/trials`, request),
    );
  }

  async createLead(request: LeadRequest): Promise<LeadCreated> {
    return this.unwrap(
      this.http.post<ApiEnvelope<LeadCreated>>(`${this.baseUrl}/public/leads`, request),
    );
  }

  async leads(query: LeadQuery = {}): Promise<PagedResult<Lead>> {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 20);

    if (query.kind) params = params.set('kind', query.kind);
    if (query.status) params = params.set('status', query.status);
    if (query.search) params = params.set('search', query.search);

    return this.unwrap(
      this.http.get<ApiEnvelope<PagedResult<Lead>>>(`${this.baseUrl}/public/leads`, {
        params,
      }),
    );
  }

  private async unwrap<T>(source: Observable<ApiEnvelope<T>>): Promise<T> {
    const response = await firstValueFrom(source);
    if (!response.success || response.data === null) {
      throw new Error(response.message ?? 'The request could not be completed.');
    }
    return response.data;
  }
}
