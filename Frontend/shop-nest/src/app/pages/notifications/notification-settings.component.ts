import { Component, OnInit, signal } from '@angular/core';

@Component({
  selector: 'app-notification-settings',
  standalone: false,
  template: `
    <section class="notification-settings-container" style="max-width: 600px; margin: 2rem auto; padding: 0 1rem;">
      <div class="panel" style="padding: 2rem;">
        <h1 style="margin: 0 0 4px; font-size: 1.75rem; font-weight: 850;">Notification Settings</h1>
        <p style="color: var(--muted); font-size: 0.9rem; margin-bottom: 2rem;">
          Configure how and when you receive order, promo, and system notifications.
        </p>

        <div style="display: flex; flex-direction: column; gap: 1.5rem;">
          <div class="setting-row" style="display: flex; justify-content: space-between; align-items: center; padding-bottom: 1rem; border-bottom: 1px solid var(--line);">
            <div>
              <h4 style="margin: 0; font-size: 1rem; font-weight: 750;">Order Status Updates</h4>
              <p style="margin: 4px 0 0; color: var(--muted); font-size: 0.85rem;">Receive live tracking updates for your active orders.</p>
            </div>
            <label class="switch" style="position: relative; display: inline-block; width: 44px; height: 24px;">
              <input type="checkbox" [(ngModel)]="orderNotifications" (change)="savePreferences()" style="opacity: 0; width: 0; height: 0;">
              <span class="slider" [style.background-color]="orderNotifications() ? 'var(--accent)' : '#ccc'" style="position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; border-radius: 24px; transition: .3s;"></span>
            </label>
          </div>

          <div class="setting-row" style="display: flex; justify-content: space-between; align-items: center; padding-bottom: 1rem; border-bottom: 1px solid var(--line);">
            <div>
              <h4 style="margin: 0; font-size: 1rem; font-weight: 750;">Promotions & Offers</h4>
              <p style="margin: 4px 0 0; color: var(--muted); font-size: 0.85rem;">Get alerted about sale promotions, coupons, and discount highlights.</p>
            </div>
            <label class="switch" style="position: relative; display: inline-block; width: 44px; height: 24px;">
              <input type="checkbox" [(ngModel)]="promotionalNotifications" (change)="savePreferences()" style="opacity: 0; width: 0; height: 0;">
              <span class="slider" [style.background-color]="promotionalNotifications() ? 'var(--accent)' : '#ccc'" style="position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; border-radius: 24px; transition: .3s;"></span>
            </label>
          </div>

          <div class="setting-row" style="display: flex; justify-content: space-between; align-items: center; padding-bottom: 1rem; border-bottom: 1px solid var(--line);">
            <div>
              <h4 style="margin: 0; font-size: 1rem; font-weight: 750;">System Bulletins</h4>
              <p style="margin: 4px 0 0; color: var(--muted); font-size: 0.85rem;">Be notified of security updates and site maintenance updates.</p>
            </div>
            <label class="switch" style="position: relative; display: inline-block; width: 44px; height: 24px;">
              <input type="checkbox" [(ngModel)]="systemNotifications" (change)="savePreferences()" style="opacity: 0; width: 0; height: 0;">
              <span class="slider" [style.background-color]="systemNotifications() ? 'var(--accent)' : '#ccc'" style="position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; border-radius: 24px; transition: .3s;"></span>
            </label>
          </div>

          <div class="setting-row" style="display: flex; justify-content: space-between; align-items: center; padding-bottom: 1rem;">
            <div>
              <h4 style="margin: 0; font-size: 1rem; font-weight: 750;">Email Notifications</h4>
              <p style="margin: 4px 0 0; color: var(--muted); font-size: 0.85rem;">Receive invoice details and email digests directly in your inbox.</p>
            </div>
            <label class="switch" style="position: relative; display: inline-block; width: 44px; height: 24px;">
              <input type="checkbox" [(ngModel)]="emailNotifications" (change)="savePreferences()" style="opacity: 0; width: 0; height: 0;">
              <span class="slider" [style.background-color]="emailNotifications() ? 'var(--accent)' : '#ccc'" style="position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; border-radius: 24px; transition: .3s;"></span>
            </label>
          </div>
        </div>

        <div style="margin-top: 2rem; background: var(--soft); padding: 1rem; border-radius: 8px; font-size: 0.85rem; color: var(--muted); text-align: center;">
          Preferences are automatically saved to your account local profile.
        </div>
      </div>
    </section>
  `
})
export class NotificationSettingsComponent implements OnInit {
  readonly orderNotifications = signal<boolean>(true);
  readonly promotionalNotifications = signal<boolean>(true);
  readonly systemNotifications = signal<boolean>(true);
  readonly emailNotifications = signal<boolean>(true);

  ngOnInit(): void {
    const raw = localStorage.getItem('shopnest.notification_preferences');
    if (raw) {
      try {
        const parsed = JSON.parse(raw);
        this.orderNotifications.set(parsed.order ?? true);
        this.promotionalNotifications.set(parsed.promotional ?? true);
        this.systemNotifications.set(parsed.system ?? true);
        this.emailNotifications.set(parsed.email ?? true);
      } catch (e) {
        console.error('Failed to parse preferences:', e);
      }
    }
  }

  savePreferences(): void {
    const payload = {
      order: this.orderNotifications(),
      promotional: this.promotionalNotifications(),
      system: this.systemNotifications(),
      email: this.emailNotifications()
    };
    localStorage.setItem('shopnest.notification_preferences', JSON.stringify(payload));
  }
}
