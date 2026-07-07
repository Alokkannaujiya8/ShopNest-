import { Component, inject } from '@angular/core';
import { BreadcrumbService } from '../../../core/breadcrumb.service';

@Component({
  selector: 'app-breadcrumb',
  standalone: false,
  template: `
    <nav class="breadcrumb-nav" aria-label="Breadcrumb" *ngIf="breadcrumbService.breadcrumbs().length > 0">
      <ol class="breadcrumb-list">
        <li class="breadcrumb-item">
          <a routerLink="/catalog">Home</a>
        </li>
        <li *ngFor="let crumb of breadcrumbService.breadcrumbs(); let last = last" class="breadcrumb-item" [class.active]="last">
          <span class="divider">/</span>
          <a *ngIf="!last; else lastLabel" [routerLink]="crumb.route">{{ crumb.label }}</a>
          <ng-template #lastLabel>
            <span aria-current="page">{{ crumb.label }}</span>
          </ng-template>
        </li>
      </ol>
    </nav>
  `,
  styles: [`
    .breadcrumb-nav {
      margin-bottom: 1rem;
    }
    .breadcrumb-list {
      display: flex;
      flex-wrap: wrap;
      list-style: none;
      padding: 0;
      margin: 0;
      font-size: 0.85rem;
      color: var(--muted);
      align-items: center;
    }
    .breadcrumb-item {
      display: inline-flex;
      align-items: center;
      gap: 6px;
    }
    .breadcrumb-item a {
      color: var(--muted);
      text-decoration: none;
      font-weight: 500;
    }
    .breadcrumb-item a:hover {
      color: var(--accent);
      text-decoration: underline;
    }
    .breadcrumb-item.active {
      color: var(--ink);
      font-weight: 600;
    }
    .divider {
      color: var(--line);
      font-size: 0.8rem;
    }
  `]
})
export class BreadcrumbComponent {
  readonly breadcrumbService = inject(BreadcrumbService);
}
