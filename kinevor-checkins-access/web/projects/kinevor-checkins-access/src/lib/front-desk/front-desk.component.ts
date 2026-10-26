import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { CheckInsFacade } from '../check-ins.facade';
import {
  formatCheckInDate,
  formatDuration,
  formatHour,
  initials,
  utilizationWidth,
} from '../check-in.utils';

/** Front-desk member search, eligibility and check-in/out console. */
@Component({
  selector: 'kinevor-front-desk',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './front-desk.component.html',
  styleUrl: './front-desk.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FrontDeskComponent implements OnInit, OnDestroy {
  readonly facade = inject(CheckInsFacade);
  readonly initials = initials;
  readonly formatDate = formatCheckInDate;
  readonly formatDuration = formatDuration;
  readonly formatHour = formatHour;
  readonly utilizationWidth = utilizationWidth;

  private searchTimer: ReturnType<typeof setTimeout> | undefined;

  /** Loads the demo front-desk workspace. */
  ngOnInit(): void {
    void this.facade.initialize();
  }

  /** Cancels pending search input work. */
  ngOnDestroy(): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
  }

  /** Applies the selected branch. */
  onBranchChange(event: Event): void {
    const branchId = (event.target as HTMLSelectElement).value;
    void this.facade.selectBranch(branchId);
  }

  /** Debounces member lookup by name, number or e-mail. */
  onSearch(event: Event): void {
    const search = (event.target as HTMLInputElement).value;
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      void this.facade.searchMembers(search);
    }, 250);
  }

  /** Loads eligibility for the chosen member. */
  selectMember(memberId: string): void {
    void this.facade.selectMember(memberId);
  }

  /** Checks the selected member in. */
  checkIn(): void {
    void this.facade.checkInSelected();
  }

  /** Closes an open visit. */
  checkOut(checkInId: string): void {
    void this.facade.checkOut(checkInId);
  }

  /** Moves the member result page. */
  movePage(delta: number): void {
    const next = this.facade.memberPage() + delta;
    if (next >= 1 && next <= this.facade.memberPageCount()) {
      void this.facade.setMemberPage(next);
    }
  }
}
