import { Component, OnInit, signal } from '@angular/core';

export interface AuditLog {
  id: string;
  user: string;
  role: string;
  action: string;
  module: string;
  entity: string;
  ipAddress: string;
  browser: string;
  timestamp: string;
  status: string;
  beforeChanges?: string;
  afterChanges?: string;
}

@Component({
  selector: 'app-admin-audit-logs',
  standalone: false,
  template: `
    <section class="audit-logs-container" style="max-width: 1000px; margin: 2rem auto; padding: 0 1rem;">
      <div class="panel" style="padding: 2rem;">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; border-bottom: 1px solid var(--line); padding-bottom: 1rem;">
          <div>
            <h1 style="margin: 0; font-size: 1.75rem; font-weight: 850;">Administrative Audit Logs</h1>
            <p style="margin: 4px 0 0; color: var(--muted); font-size: 0.9rem;">
              Monitor security events and operations audit history
            </p>
          </div>
          <button class="ghost" (click)="clearLogs()" [disabled]="logs().length === 0">
            Clear Logs
          </button>
        </div>

        <div *ngIf="logs().length === 0" style="text-align: center; padding: 4rem 0;">
          <div style="font-size: 3rem; margin-bottom: 1rem;">🛡️</div>
          <h3>All secure!</h3>
          <p style="color: var(--muted); margin: 4px 0 0;">No security audit events have been logged yet.</p>
        </div>

        <div class="table-panel panel" *ngIf="logs().length > 0">
          <div class="table-row head" style="display: grid; grid-template-columns: 1.5fr 1fr 1.5fr 1.5fr 1fr 1.5fr; font-weight: bold; border-bottom: 2px solid var(--line); padding-bottom: 8px;">
            <span>Timestamp</span>
            <span>User</span>
            <span>Module</span>
            <span>Action</span>
            <span>IP Address</span>
            <span>Status</span>
          </div>
          <div 
            class="table-row" 
            *ngFor="let log of logs()" 
            style="display: grid; grid-template-columns: 1.5fr 1fr 1.5fr 1.5fr 1fr 1.5fr; padding: 12px 0; border-bottom: 1px solid var(--line); font-size: 0.9rem; align-items: center;"
          >
            <span>{{ log.timestamp | date:'short' }}</span>
            <span>{{ log.user }} ({{ log.role }})</span>
            <span><strong>{{ log.module }}</strong></span>
            <span>{{ log.action }}</span>
            <span>{{ log.ipAddress }}</span>
            <span>
              <span 
                class="tag" 
                [style.background]="log.status === 'Success' ? '#e8f5e9' : '#ffebee'"
                [style.color]="log.status === 'Success' ? '#2e7d32' : '#c62828'"
                style="padding: 4px 8px; border-radius: 4px; font-size: 0.75rem;"
              >
                {{ log.status }}
              </span>
            </span>
          </div>
        </div>
      </div>
    </section>
  `
})
export class AdminAuditLogsComponent implements OnInit {
  readonly logs = signal<AuditLog[]>([]);

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    const raw = localStorage.getItem('shopnest.audit_logs');
    if (raw) {
      try {
        this.logs.set(JSON.parse(raw));
      } catch (e) {
        console.error('Failed to parse audit logs:', e);
      }
    } else {
      // Seed default logs if empty
      const defaultLogs: AuditLog[] = [
        {
          id: '1',
          user: 'admin@shopnest.com',
          role: 'Administrator',
          action: 'User Authentication',
          module: 'Security',
          entity: 'UserSession',
          ipAddress: '127.0.0.1',
          browser: 'Chrome 124.0.0',
          timestamp: new Date(Date.now() - 300000).toISOString(),
          status: 'Success'
        },
        {
          id: '2',
          user: 'admin@shopnest.com',
          role: 'Administrator',
          action: 'Update Inventory Limits',
          module: 'Inventory',
          entity: 'WarehouseItem',
          ipAddress: '127.0.0.1',
          browser: 'Chrome 124.0.0',
          timestamp: new Date(Date.now() - 600000).toISOString(),
          status: 'Success'
        }
      ];
      localStorage.setItem('shopnest.audit_logs', JSON.stringify(defaultLogs));
      this.logs.set(defaultLogs);
    }
  }

  clearLogs(): void {
    localStorage.removeItem('shopnest.audit_logs');
    this.logs.set([]);
  }
}
