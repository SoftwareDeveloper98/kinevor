import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';
import { NoticeService, PagedResult } from '@softwaredeveloper98/kinevor-platform-web';
import { TrainerSummary } from './trainer.model';
import { TRAINERS_GATEWAY, TrainersGateway } from './trainers.gateway';
import { TrainersFacade } from './trainers.facade';

const trainer: TrainerSummary = {
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
};

const page: PagedResult<TrainerSummary> = {
  items: [trainer],
  totalCount: 1,
  page: 1,
  pageSize: 9,
  totalPages: 1,
  hasNext: false,
  hasPrev: false,
};

function setup() {
  const gateway: TrainersGateway = {
    list: vi.fn(() => of(page)),
    byId: vi.fn(() => of(trainer)),
  };
  const notice = { error: vi.fn() };

  TestBed.configureTestingModule({
    providers: [
      TrainersFacade,
      { provide: TRAINERS_GATEWAY, useValue: gateway },
      { provide: NoticeService, useValue: notice },
    ],
  });

  return { facade: TestBed.inject(TrainersFacade), gateway, notice };
}

describe('TrainersFacade', () => {
  it('loads the first page into state', () => {
    const { facade, gateway } = setup();

    facade.load();

    expect(gateway.list).toHaveBeenCalledWith({ page: 1, pageSize: 9 });
    expect(facade.trainers()).toEqual([trainer]);
    expect(facade.total()).toBe(1);
    expect(facade.loading()).toBe(false);
  });

  it('merges query updates and reloads', () => {
    const { facade, gateway } = setup();

    facade.updateQuery({ page: 2, search: 'nour' });

    expect(facade.query()).toEqual({ page: 2, pageSize: 9, search: 'nour' });
    expect(gateway.list).toHaveBeenCalledWith({ page: 2, pageSize: 9, search: 'nour' });
  });

  it('loads a profile into detail state', () => {
    const { facade } = setup();

    facade.loadDetail('t1').subscribe();

    expect(facade.detail()).toEqual(trainer);
    expect(facade.detailLoading()).toBe(false);
  });

  it('reports load failures through the notice service', () => {
    const { facade, gateway, notice } = setup();
    gateway.list = vi.fn(() => throwError(() => new Error('offline')));

    facade.load();

    expect(facade.loading()).toBe(false);
    expect(notice.error).toHaveBeenCalledWith(null, 'error.unknown');
  });

  it('exposes a picker page as an array', () => {
    const { facade } = setup();

    let result: TrainerSummary[] = [];
    facade.forPicker().subscribe(items => (result = items));

    expect(result).toEqual([trainer]);
  });
});
