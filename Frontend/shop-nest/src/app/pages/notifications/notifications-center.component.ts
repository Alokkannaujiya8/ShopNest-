import { Component, OnInit, inject, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { SignalrService } from '../../core/signalr.service';
import { Notification } from '../../core/api.models';

@Component({
  selector: 'app-notifications-center',
  standalone: false,
  template: `
    <section class="notifications-container" style="max-width: 800px; margin: 2rem auto; padding: 0 1rem;">
      <div class="panel" style="padding: 2rem;">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; border-bottom: 1px solid var(--line); padding-bottom: 1rem;">
          <div>
            <h1 style="margin: 0; font-size: 1.75rem; font-weight: 850;">Notification Center</h1>
            <p style="margin: 4px 0 0; color: var(--muted); font-size: 0.9rem;">
              Manage your notifications and system updates
            </p>
          </div>
          <button class="ghost" (click)="markAllAsRead()" [disabled]="loading() || unreadCount() === 0">
            Mark All as Read
          </button>
        </div>

        <div *ngIf="loading() && list().length === 0" style="text-align: center; padding: 3rem 0;">
          <p style="color: var(--muted);">Loading notifications...</p>
        </div>

        <div *ngIf="!loading() && list().length === 0" style="text-align: center; padding: 4rem 0;">
          <div style="font-size: 3rem; margin-bottom: 1rem;">🔔</div>
          <h3>All caught up!</h3>
          <p style="color: var(--muted); margin: 4px 0 0;">You have no active notifications.</p>
        </div>

        <div class="notifications-list" *ngIf="list().length > 0" style="display: flex; flex-direction: column; gap: 12px;">
          <div 
            class="notification-item panel" 
            *ngFor="let n of list()" 
            [class.unread]="!n.isRead"
            style="padding: 1.25rem; display: flex; gap: 1rem; align-items: start; border: 1px solid var(--line); position: relative; transition: background-color 0.2s;"
            [style.background]="!n.isRead ? 'var(--soft)' : 'var(--bg)'"
          >
            <div class="icon-box" style="font-size: 1.5rem; background: var(--soft); padding: 8px; border-radius: 50%;">
              {{ getIcon(n.notificationType) }}
            </div>
            
            <div style="flex-grow: 1; min-width: 0;">
              <div style="display: flex; justify-content: space-between; align-items: start; gap: 12px;">
                <h4 style="margin: 0; font-size: 1.05rem; font-weight: 750; color: var(--ink);">{{ n.title }}</h4>
                <span style="font-size: 0.75rem; color: var(--muted); white-space: nowrap;">
                  {{ n.sentTime | date:'shortTime' }}
                </span>
              </div>
              <p style="margin: 6px 0 0; color: var(--muted); font-size: 0.9rem; line-height: 1.4;">{{ n.message }}</p>
              
              <div style="display: flex; gap: 12px; margin-top: 10px;">
                <button 
                  *ngIf="!n.isRead" 
                  class="ghost" 
                  style="font-size: 0.75rem; padding: 4px 8px; height: auto;" 
                  (click)="markAsRead(n)"
                >
                  Mark as Read
                </button>
                <button 
                  class="ghost" 
                  style="font-size: 0.75rem; padding: 4px 8px; height: auto; border-color: #d32f2f; color: #d32f2f;" 
                  (click)="deleteNotification(n.id)"
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  `
})
export class NotificationsCenterComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly signalr = inject(SignalrService);

  readonly list = signal<Notification[]>([]);
  readonly unreadCount = signal<number>(0);
  readonly loading = signal<boolean>(false);

  ngOnInit(): void {
    this.loadNotifications();
    this.loadUnreadCount();

    // Listen to real-time incoming notifications
    this.signalr.notificationReceived$.subscribe((notif) => {
      this.list.update((prev) => [notif, ...prev]);
      this.unreadCount.update((count) => count + 1);
    });
  }

  loadNotifications(): void {
    this.loading.set(true);
    this.api.getNotifications(undefined, 'InApp', undefined, undefined, undefined, undefined, undefined, 1).subscribe({
      next: (res) => {
        this.list.set(res.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadUnreadCount(): void {
    this.api.getUnreadNotificationsCount().subscribe({
      next: (count) => this.unreadCount.set(count)
    });
  }

  markAsRead(item: Notification): void {
    if (item.isRead) return;
    this.api.markNotificationAsRead(item.id).subscribe({
      next: () => {
        item.isRead = true;
        this.unreadCount.update((count) => Math.max(0, count - 1));
      }
    });
  }

  markAllAsRead(): void {
    this.api.markAllNotificationsAsRead().subscribe({
      next: () => {
        this.list.update((prev) => {
          prev.forEach((n) => (n.isRead = true));
          return [...prev];
        });
        this.unreadCount.set(0);
      }
    });
  }

  deleteNotification(id: string): void {
    this.api.deleteNotification(id).subscribe({
      next: () => {
        this.list.update((prev) => prev.filter((n) => n.id !== id));
        this.loadUnreadCount();
      }
    });
  }

  getIcon(type: string): string {
    switch (type) {
      case 'OrderPlaced':
      case 'OrderConfirmed':
      case 'OrderPacked':
      case 'OrderShipped':
      case 'OutForDelivery':
      case 'OrderDelivered':
        return '📦';
      case 'PaymentSuccess':
        return '💳';
      case 'PaymentFailed':
        return '❌';
      case 'Promotional':
        return '🎉';
      default:
        return '✉️';
    }
  }
}
