import { Injectable, computed, inject, signal } from '@angular/core';
import {
  AccessMemberSummary,
  Branch,
  CHECKINS_GATEWAY,
  CheckInHistoryQuery,
  CheckInVisit,
  DailyStats,
  Eligibility,
  Occupancy,
  PagedResult,
} from './check-in.model';
import { checkInErrorMessage } from './check-in.utils';

/** Signal facade for the front-desk member lookup, eligibility and check-in workflow. */
@Injectable()
export class CheckInsFacade {
  private readonly gateway = inject(CHECKINS_GATEWAY);

  readonly branches = signal<Branch[]>([]);
  readonly branchId = signal<string | null>(null);
  readonly members = signal<AccessMemberSummary[]>([]);
  readonly memberTotal = signal(0);
  readonly memberPage = signal(1);
  readonly memberPageSize = 6;
  readonly memberSearch = signal('');
  readonly selectedMemberId = signal<string | null>(null);
  readonly eligibility = signal<Eligibility | null>(null);
  readonly occupancy = signal<Occupancy | null>(null);
  readonly stats = signal<DailyStats | null>(null);
  readonly history = signal<PagedResult<CheckInVisit> | null>(null);
  readonly loading = signal(false);
  readonly eligibilityLoading = signal(false);
  readonly actionLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);

  readonly branch = computed(
    () => this.branches().find((item) => item.id === this.branchId()) ?? null,
  );
  readonly selectedMember = computed(
    () => this.members().find((item) => item.id === this.selectedMemberId()) ?? null,
  );
  readonly openVisits = computed(() => this.occupancy()?.openVisits ?? []);
  readonly memberPageCount = computed(() =>
    Math.max(1, Math.ceil(this.memberTotal() / this.memberPageSize)),
  );

  /** Loads branches and the first branch's front-desk workspace. */
  async initialize(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const branches = await this.gateway.branches();
      this.branches.set(branches);
      const preferred = branches.find((item) => item.code === 'SHR') ?? branches.at(0) ?? null;
      this.branchId.set(preferred?.id ?? null);
      await this.loadBranchWorkspace();
      const firstMember = this.members().at(0);
      if (firstMember) await this.selectMember(firstMember.id);
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  /** Changes the active front-desk branch and refreshes its workspace. */
  async selectBranch(branchId: string): Promise<void> {
    if (branchId === this.branchId()) return;
    this.branchId.set(branchId);
    this.selectedMemberId.set(null);
    this.eligibility.set(null);
    this.loading.set(true);
    try {
      await this.loadBranchWorkspace();
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  /** Runs a member search and returns to page one. */
  async searchMembers(search: string): Promise<void> {
    this.memberSearch.set(search);
    this.memberPage.set(1);
    await this.loadMembers();
  }

  /** Moves the member result page. */
  async setMemberPage(page: number): Promise<void> {
    this.memberPage.set(page);
    await this.loadMembers();
  }

  /** Loads the eligibility panel for a member. */
  async selectMember(memberId: string): Promise<void> {
    this.selectedMemberId.set(memberId);
    this.eligibilityLoading.set(true);
    try {
      this.eligibility.set(await this.gateway.eligibility(memberId, this.branchId()));
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
      this.eligibility.set(null);
    } finally {
      this.eligibilityLoading.set(false);
    }
  }

  /** Re-evaluates the selected member against the current branch. */
  async refreshEligibility(): Promise<void> {
    const memberId = this.selectedMemberId();
    if (memberId) await this.selectMember(memberId);
  }

  /** Checks in the selected member at the active branch. */
  async checkInSelected(notes?: string | null): Promise<boolean> {
    const member = this.selectedMember();
    if (!member || !this.branchId()) return false;

    this.actionLoading.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      const visit = await this.gateway.checkIn({
        memberId: member.id,
        branchId: this.branchId(),
        notes: notes ?? null,
      });
      this.notice.set(`${visit.memberName} checked in at ${visit.branchName}.`);
      await this.refreshAfterVisit();
      return true;
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
      await this.refreshAfterVisit();
      return false;
    } finally {
      this.actionLoading.set(false);
    }
  }

  /** Closes an open visit. */
  async checkOut(checkInId: string): Promise<boolean> {
    this.actionLoading.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      const visit = await this.gateway.checkOut(checkInId);
      this.notice.set(`${visit.memberName} checked out of ${visit.branchName}.`);
      await this.refreshAfterVisit();
      return true;
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
      await this.refreshAfterVisit();
      return false;
    } finally {
      this.actionLoading.set(false);
    }
  }

  /** Dismisses the current notice. */
  clearNotice(): void {
    this.notice.set(null);
  }

  /** Dismisses the current error. */
  clearError(): void {
    this.error.set(null);
  }

  private async loadBranchWorkspace(): Promise<void> {
    await Promise.all([
      this.loadMembers(),
      this.loadOccupancy(),
      this.loadStats(),
      this.loadHistory(),
    ]);
  }

  private async loadMembers(): Promise<void> {
    const result = await this.gateway.members({
      search: this.memberSearch(),
      page: this.memberPage(),
      pageSize: this.memberPageSize,
    });
    this.members.set(result.items);
    this.memberTotal.set(result.totalCount);
  }

  private async loadOccupancy(): Promise<void> {
    this.occupancy.set(await this.gateway.occupancy(this.branchId()));
  }

  private async loadStats(): Promise<void> {
    this.stats.set(await this.gateway.stats(this.branchId()));
  }

  private async loadHistory(): Promise<void> {
    const query: CheckInHistoryQuery = {
      branchId: this.branchId() ?? undefined,
      page: 1,
      pageSize: 6,
    };
    this.history.set(await this.gateway.history(query));
  }

  private async refreshAfterVisit(): Promise<void> {
    try {
      await Promise.all([
        this.loadMembers(),
        this.loadOccupancy(),
        this.loadStats(),
        this.loadHistory(),
      ]);
      await this.refreshEligibility();
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
    }
  }
}
