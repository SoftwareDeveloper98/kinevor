import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { KINEVOR_API_BASE_URL } from '@softwaredeveloper98/kinevor-platform-web';
import { HttpTrainersGateway } from './http-trainers.gateway';

const envelope = {
  success: true,
  data: {
    items: [
      {
        id: 't1',
        fullName: 'Nour Adel',
        email: 'nour.adel@kinevor.demo',
        phone: '+201003456789',
        bio: 'Coach',
        specializations: ['Spinning'],
        rating: 4.9,
        totalRatings: 143,
        yearsExperience: 5,
        status: 'Active',
      },
    ],
    totalCount: 1,
    page: 2,
    pageSize: 10,
    totalPages: 1,
    hasNext: false,
    hasPrev: true,
  },
  message: null,
  errors: null,
};

describe('HttpTrainersGateway', () => {
  let gateway: HttpTrainersGateway;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        HttpTrainersGateway,
        { provide: KINEVOR_API_BASE_URL, useValue: 'http://api.test/api/v1' },
      ],
    });
    gateway = TestBed.inject(HttpTrainersGateway);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('unwraps a paged trainer response', () => {
    let total = 0;
    gateway.list({ page: 2, pageSize: 10, search: 'nour' }).subscribe(page => {
      total = page.totalCount;
      expect(page.items[0].fullName).toBe('Nour Adel');
    });

    const request = http.expectOne(
      req => req.url === 'http://api.test/api/v1/trainers',
    );
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('10');
    expect(request.request.params.get('search')).toBe('nour');
    request.flush(envelope);

    expect(total).toBe(1);
  });

  it('unwraps a single trainer response', () => {
    let name = '';
    gateway.byId('t1').subscribe(trainer => (name = trainer.fullName));

    const request = http.expectOne('http://api.test/api/v1/trainers/t1');
    request.flush({ success: true, data: envelope.data.items[0], message: null, errors: null });

    expect(name).toBe('Nour Adel');
  });
});
