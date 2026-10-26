import { InjectionToken } from '@angular/core';

export interface ApiEnvelope<T> {
  success: boolean;
  data: T | null;
  message?: string | null;
  errors?: string[] | null;
}

export interface Branch {
  id: string;
  name: string;
  slug: string;
  neighbourhood: string;
  address: string;
  city: string;
  phone: string;
  openingHours: string;
  parking: string;
  amenities: string[];
  capacity: number;
  latitude: number;
  longitude: number;
  theme: string;
}

export interface PublicSite {
  name: string;
  tagline: string;
  description: string;
  story: string;
  eyebrow: string;
  phone: string;
  email: string;
  address: string;
  city: string;
  instagramHandle: string;
  memberCount: number;
  trainerCount: number;
  weeklyClasses: number;
  rating: number;
  foundedYear: number;
  highlights: string[];
  branches: Branch[];
}

export interface MembershipPlan {
  id: string;
  name: string;
  slug: string;
  description: string;
  tier: string;
  price: number;
  currency: string;
  billingPeriodMonths: number;
  billingLabel: string;
  features: string[];
  badge: string | null;
  isFeatured: boolean;
}

export interface Trainer {
  id: string;
  fullName: string;
  slug: string;
  roleTitle: string;
  bio: string;
  specializations: string[];
  certifications: string[];
  yearsExperience: number;
  branchName: string | null;
  photoUrl: string | null;
}

export interface ClassSession {
  id: string;
  name: string;
  description: string;
  category: string;
  intensity: string;
  trainerId: string;
  trainerName: string;
  startsAt: string;
  durationMinutes: number;
  capacity: number;
  room: string;
  branchId: string;
  branchName: string;
}

export interface Faq {
  id: string;
  category: string;
  question: string;
  answer: string;
}

export interface BlogPostSummary {
  id: string;
  slug: string;
  title: string;
  excerpt: string;
  author: string;
  publishedAt: string;
  imageUrl: string | null;
}

export interface BlogPost extends BlogPostSummary {
  content: string;
}

export interface ScheduleQuery {
  from?: string;
  to?: string;
  branchId?: string;
}

export interface TrialRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  preferredDate: string | null;
  classSessionId: string | null;
  branchId: string | null;
  goal: string | null;
  notes: string | null;
  consentToContact: boolean;
}

export interface TrialCreated {
  id: string;
  message: string;
}

export interface LeadRequest {
  name: string;
  email: string;
  phone: string | null;
  interest: string;
  message: string;
  consentToContact: boolean;
}

export interface LeadCreated {
  id: string;
  message: string;
}

export interface Lead {
  id: string;
  kind: 'Trial' | 'Enquiry';
  name: string;
  email: string;
  phone: string | null;
  interest: string | null;
  message: string | null;
  preferredDate: string | null;
  className: string | null;
  branchName: string | null;
  status: string;
  createdAt: string;
}

export interface LeadQuery {
  kind?: 'Trial' | 'Enquiry';
  status?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrev: boolean;
}

export interface PublicSiteGateway {
  site(): Promise<PublicSite>;
  branches(): Promise<Branch[]>;
  branch(slug: string): Promise<Branch>;
  plans(): Promise<MembershipPlan[]>;
  trainers(): Promise<Trainer[]>;
  schedule(query?: ScheduleQuery): Promise<ClassSession[]>;
  faqs(): Promise<Faq[]>;
  posts(): Promise<BlogPostSummary[]>;
  post(slug: string): Promise<BlogPost>;
  createTrial(request: TrialRequest): Promise<TrialCreated>;
  createLead(request: LeadRequest): Promise<LeadCreated>;
  leads(query?: LeadQuery): Promise<PagedResult<Lead>>;
}

export interface PublicSiteOptions {
  apiBaseUrl: string;
}

export const PUBLIC_SITE_API_BASE_URL = new InjectionToken<string>(
  'PUBLIC_SITE_API_BASE_URL',
);

export const PUBLIC_SITE_GATEWAY = new InjectionToken<PublicSiteGateway>(
  'PUBLIC_SITE_GATEWAY',
);
