import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../core/api.service';
import { NotificationTemplate, NotificationLog } from '../../../core/api.models';

@Component({
  selector: 'app-admin-notifications',
  standalone: false,
  templateUrl: './admin-notifications.component.html',
  styleUrl: './admin-notifications.component.scss'
})
export class AdminNotificationsComponent implements OnInit {
  templates: NotificationTemplate[] = [];
  logs: NotificationLog[] = [];
  users: any[] = [];

  // Active view tab: 'send', 'templates', 'logs'
  activeTab = 'send';

  // Manual Notification Form
  manualTargetUserId = '';
  manualTitle = '';
  manualMessage = '';
  manualType = 'Information';
  manualChannel = 'InApp';
  manualPriority = 'Medium';

  // Broadcast Form
  broadcastTitle = '';
  broadcastMessage = '';
  broadcastType = 'Information';
  broadcastChannel = 'InApp';
  broadcastPriority = 'Medium';

  // Template Form
  selectedTemplate?: NotificationTemplate;
  templateCode = '';
  templateName = '';
  templateSubject = '';
  templateBody = '';
  templateChannel = 'Email';
  isEditingTemplate = false;

  // Logs Search/Filters
  searchQuery = '';
  logPage = 1;
  logTotalCount = 0;

  errorMsg = '';
  notice = '';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.loadTemplates();
    this.loadLogs();
    this.loadUsers();
  }

  loadTemplates(): void {
    this.api.getNotificationTemplates().subscribe({
      next: (res) => this.templates = res,
      error: (err) => this.errorMsg = 'Failed to load templates.'
    });
  }

  loadLogs(): void {
    this.api.getNotificationLogs(undefined, this.logPage).subscribe({
      next: (res) => {
        this.logs = res.items;
        this.logTotalCount = res.totalCount;
      },
      error: () => this.errorMsg = 'Failed to load delivery logs.'
    });
  }

  loadUsers(): void {
    this.api.getAdminUsers({ page: 1, pageSize: 200 }).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.users = res.data.items;
        }
      }
    });
  }

  sendManual(): void {
    if (!this.manualTargetUserId || !this.manualTitle || !this.manualMessage) {
      this.errorMsg = 'Please fill out all fields.';
      return;
    }
    this.errorMsg = '';

    this.api.sendManualNotification({
      userId: this.manualTargetUserId,
      title: this.manualTitle,
      message: this.manualMessage,
      notificationType: this.manualType,
      channel: this.manualChannel,
      priority: this.manualPriority
    }).subscribe({
      next: () => {
        this.showNotice('Manual notification sent successfully!');
        this.manualTitle = '';
        this.manualMessage = '';
        this.loadLogs();
      },
      error: (err) => this.errorMsg = 'Failed to send: ' + (err.error || err.message)
    });
  }

  sendBroadcast(): void {
    if (!this.broadcastTitle || !this.broadcastMessage) {
      this.errorMsg = 'Please fill out title and message.';
      return;
    }
    this.errorMsg = '';

    this.api.broadcastNotification({
      title: this.broadcastTitle,
      message: this.broadcastMessage,
      notificationType: this.broadcastType,
      channel: this.broadcastChannel,
      priority: this.broadcastPriority
    }).subscribe({
      next: () => {
        this.showNotice('Broadcast notification dispatched to all active users!');
        this.broadcastTitle = '';
        this.broadcastMessage = '';
        this.loadLogs();
      },
      error: (err) => this.errorMsg = 'Failed to broadcast: ' + (err.error || err.message)
    });
  }

  selectTemplate(t: NotificationTemplate): void {
    this.selectedTemplate = t;
    this.templateCode = t.code;
    this.templateName = t.name;
    this.templateSubject = t.subject;
    this.templateBody = t.body;
    this.templateChannel = t.channel;
    this.isEditingTemplate = true;
  }

  resetTemplateForm(): void {
    this.selectedTemplate = undefined;
    this.templateCode = '';
    this.templateName = '';
    this.templateSubject = '';
    this.templateBody = '';
    this.templateChannel = 'Email';
    this.isEditingTemplate = false;
  }

  saveTemplate(): void {
    if (!this.templateCode || !this.templateName || !this.templateSubject || !this.templateBody) {
      this.errorMsg = 'Please fill out all fields.';
      return;
    }
    this.errorMsg = '';

    if (this.isEditingTemplate) {
      this.api.updateNotificationTemplate(this.templateCode, {
        name: this.templateName,
        subject: this.templateSubject,
        body: this.templateBody,
        channel: this.templateChannel
      }).subscribe({
        next: () => {
          this.showNotice('Notification template updated.');
          this.resetTemplateForm();
          this.loadTemplates();
        },
        error: (err) => this.errorMsg = 'Failed to update: ' + (err.error || err.message)
      });
    } else {
      this.api.createNotificationTemplate({
        code: this.templateCode,
        name: this.templateName,
        subject: this.templateSubject,
        body: this.templateBody,
        channel: this.templateChannel
      }).subscribe({
        next: () => {
          this.showNotice('Template created successfully.');
          this.resetTemplateForm();
          this.loadTemplates();
        },
        error: (err) => this.errorMsg = 'Failed to create: ' + (err.error || err.message)
      });
    }
  }

  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 3000);
  }
}
