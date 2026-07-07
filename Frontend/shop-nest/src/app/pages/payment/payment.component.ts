import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Order, Payment } from '../../core/api.models';

@Component({
  selector: 'app-payment',
  standalone: false,
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.scss'
})
export class PaymentComponent implements OnInit {
  orderId = '';
  paymentId = '';
  provider = '';
  amount = 0;
  currency = 'USD';

  // State flags
  loading = false;
  processing = false;
  success = false;
  failed = false;
  errorMsg = '';
  notice = '';

  // Receipt details
  transactionId = '';
  receiptDownloaded = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly api: ApiService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.orderId = params['orderId'] || '';
      this.paymentId = params['paymentId'] || '';
      this.provider = params['provider'] || 'Stripe';
      this.amount = +params['amount'] || 0;
      this.currency = params['currency'] || 'USD';

      if (!this.orderId || !this.paymentId) {
        this.errorMsg = 'Invalid payment session details. Missing parameters.';
      }
    });
  }

  processMockPayment(): void {
    this.processing = true;
    this.errorMsg = '';

    // Mock different provider experiences
    setTimeout(() => {
      this.transactionId = `${this.provider.substring(0, 3).toLowerCase()}_tx_${Math.random().toString(36).substring(2, 10)}`;
      
      // Verify payment with backend
      this.api.verifyPayment({
        paymentId: this.paymentId,
        transactionId: this.transactionId
      }).subscribe({
        next: (verified) => {
          this.processing = false;
          if (verified) {
            this.success = true;
            this.showNotice('Payment processed successfully!');
          } else {
            this.failed = true;
            this.errorMsg = 'Payment verification failed.';
          }
        },
        error: (err) => {
          this.processing = false;
          this.failed = true;
          this.errorMsg = 'Payment error: ' + (err.error || err.message);
        }
      });
    }, 2500); // simulate network latency
  }

  downloadReceipt(): void {
    this.receiptDownloaded = true;
    this.showNotice('Receipt downloaded successfully!');
    // Trigger virtual text file download
    const content = `SHOPNEST INVOICE RECEIPT\n` +
                    `=======================\n` +
                    `Payment ID: ${this.paymentId}\n` +
                    `Transaction ID: ${this.transactionId}\n` +
                    `Gateway Provider: ${this.provider}\n` +
                    `Total Amount Paid: ${this.amount} ${this.currency.toUpperCase()}\n` +
                    `Status: Captured & Succeeded\n` +
                    `Date: ${new Date().toLocaleString()}\n` +
                    `=======================\n` +
                    `Thank you for shopping with ShopNest!`;
                    
    const blob = new Blob([content], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `receipt_${this.transactionId}.txt`;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 3000);
  }
}
