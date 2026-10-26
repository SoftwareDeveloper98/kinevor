import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  formatCheckInDate,
  formatDuration,
  formatHour,
  initials,
  utilizationWidth,
} from '../check-in.utils';
import { OccupancyFacade } from '../occupancy.facade';

/** Live occupancy, today's traffic and open-visit operations. */
@Component({
  selector: 'kinevor-occupancy-dashboard',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './occupancy-dashboard.component.html',
  styleUrl: './occupancy-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OccupancyDashboardComponent implements OnInit {
  readonly facade = inject(OccupancyFacade);
  readonly initials = initials;
  readonly formatDate = formatCheckInDate;
  readonly formatDuration = formatDuration;
  readonly formatHour = formatHour;
  readonly utilizationWidth = utilizationWidth;

  /** Loads branches, occupancy and daily statistics. */
  ngOnInit(): void {
    void this.facade.initialize();
  }

  /** Switches the active branch. */
  onBranchChange(event: Event): void {
    void this.facade.selectBranch((event.target as HTMLSelectElement).value);
  }

  /** Reloads the dashboard. */
  refresh(): void {
    void this.facade.refresh();
  }

  /** Checks a member out from the live list. */
  checkOut(checkInId: string): void {
    void this.facade.checkOut(checkInId);
  }

  /** Maps occupancy pressure to a gauge colour. */
  gaugeColor(): string {
    switch (this.facade.occupancy()?.status) {
      case 'Full':
        return 'var(--color-error)';
      case 'NearlyFull':
        return 'var(--color-warning)';
      case 'Busy':
        return 'var(--color-accent)';
      default:
        return 'var(--color-success)';
    }
  }
}
