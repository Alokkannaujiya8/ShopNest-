import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { SignalrService } from '../../core/signalr.service';
import { Notification } from '../../core/api.models';

@Component({
  selector: 'app-notification-drawer',
  standalone: false,
  templateUrl: './notification-drawer.component.html',
  styleUrl: './notification-drawer.component.scss'
})
export class NotificationDrawerComponent implements OnInit {
  @Input() isOpen = false;
  @Output() closeDrawer = new EventEmitter<void>();

  notifications: Notification[] = [];
  unreadCount = 0;
  
  loading = false;
  errorMsg = '';

  constructor(
    private readonly api: ApiService,
    private readonly signalr: SignalrService
  ) {}

  ngOnInit(): void {
    if (this.api.currentUser() !== null) {
      this.loadNotifications();
      this.loadUnreadCount();

      // Listen to real-time incoming notifications
      this.signalr.notificationReceived$.subscribe((notif) => {
        // Prepend to list
        this.notifications = [notif, ...this.notifications].slice(0, 50);
        this.unreadCount++;
      });
    }
  }

  loadNotifications(): void {
    this.loading = true;
    this.api.getNotifications(undefined, 'InApp', undefined, undefined, undefined, undefined, undefined, 1).subscribe({
      next: (res) => {
        this.notifications = res.items;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  loadUnreadCount(): void {
    this.api.getUnreadNotificationsCount().subscribe({
      next: (count) => this.unreadCount = count
    });
  }

  markAsRead(item: Notification): void {
    if (item.isRead) return;
    this.api.markNotificationAsRead(item.id).subscribe({
      next: () => {
        item.isRead = true;
        this.unreadCount = Math.max(0, this.unreadCount - 1);
      }
    });
  }

  markAllAsRead(): void {
    this.api.markAllNotificationsAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
        this.unreadCount = 0;
      }
    });
  }

  deleteNotification(id: string): void {
    this.api.deleteNotification(id).subscribe({
      next: () => {
        this.notifications = this.notifications.filter(n => n.id !== id);
        this.loadUnreadCount();
      }
    });
  }

  onClose(): void {
    this.closeDrawer.emit();
  }
}
