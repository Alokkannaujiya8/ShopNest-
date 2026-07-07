import { inject, Injectable, signal, effect } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { ApiService } from './api.service';

export interface ConnectionDetails {
  connectionId: string;
  userId: string | null;
  fullName: string;
  role: string;
  loginTime: string;
  lastActivityTime: string;
}

export interface OnlineDashboardStats {
  totalOnlineUsers: number;
  activeConnections: number;
  loggedInCustomers: number;
  loggedOutCustomers: number;
  activeUsersList: ConnectionDetails[];
}

export interface HubNotification {
  type: string;
  message: string;
  timestamp: string;
}

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private readonly api = inject(ApiService);
  private hubConnection?: signalR.HubConnection;
  
  readonly onlineStats = signal<OnlineDashboardStats | null>(null);
  readonly notifications = signal<HubNotification[]>([]);
  readonly orderStatusChanged$ = new Subject<any>();
  readonly notificationReceived$ = new Subject<any>();
  readonly unreadCount = signal<number>(0);
  readonly connectionState = signal<'Connected' | 'Disconnected' | 'Connecting' | 'Reconnecting'>('Disconnected');

  private heartbeatInterval: any;

  constructor() {
    effect(() => {
      const user = this.api.currentUser();
      if (user) {
        this.connect();
      } else {
        void this.disconnect();
      }
    });
  }

  init(): void {}

  private connect(): void {
    if (this.hubConnection) {
      void this.disconnect();
    }

    const token = this.api.currentUser()?.accessToken;

    this.connectionState.set('Connecting');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7002/hubs/orders', {
        accessTokenFactory: () => token || '',
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.onreconnecting((error) => {
      this.connectionState.set('Reconnecting');
    });

    this.hubConnection.onreconnected((connectionId) => {
      this.connectionState.set('Connected');
    });

    this.hubConnection.onclose((error) => {
      this.connectionState.set('Disconnected');
    });

    this.hubConnection.on('onlineStatsUpdated', (stats: OnlineDashboardStats) => {
      this.onlineStats.set(stats);
    });

    this.hubConnection.on('notificationReceived', (notification: any) => {
      this.notifications.update((prev) => [notification, ...prev].slice(0, 50));
      this.notificationReceived$.next(notification);
    });

    this.hubConnection.on('orderStatusChanged', (order: any) => {
      this.orderStatusChanged$.next(order);
    });

    this.hubConnection.on('unreadCountUpdated', (count: number) => {
      this.unreadCount.set(count);
    });

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connection established.');
        this.connectionState.set('Connected');

        this.api.getUnreadNotificationsCount().subscribe({
          next: (count) => this.unreadCount.set(count)
        });
        
        this.heartbeatInterval = setInterval(() => {
          if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            void this.hubConnection.invoke('Heartbeat');
          }
        }, 15000);
      })
      .catch((err: any) => {
        console.error('Error starting SignalR connection:', err);
        this.connectionState.set('Disconnected');
      });
  }

  async disconnect(): Promise<void> {
    if (this.heartbeatInterval) {
      clearInterval(this.heartbeatInterval);
    }
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = undefined;
      console.log('SignalR disconnected.');
    }
    this.connectionState.set('Disconnected');
  }
}
