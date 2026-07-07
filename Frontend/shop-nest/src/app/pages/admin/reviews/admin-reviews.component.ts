import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../core/api.service';
import { Review } from '../../../core/api.models';

@Component({
  selector: 'app-admin-reviews',
  standalone: false,
  templateUrl: './admin-reviews.component.html',
  styleUrl: './admin-reviews.component.scss'
})
export class AdminReviewsComponent implements OnInit {
  reviews: Review[] = [];
  
  // Selection
  selectedReview?: Review;
  adminNotes = '';
  
  // Search & Filter
  searchQuery = '';
  statusFilter = '';
  ratingFilter = '';
  
  page = 1;
  totalItems = 0;

  errorMsg = '';
  notice = '';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    this.errorMsg = '';
    this.api.getReviews(
      undefined, 
      undefined, 
      this.statusFilter || undefined, 
      this.page
    ).subscribe({
      next: (res) => {
        this.reviews = res.items;
        this.totalItems = res.totalCount;
        this.filterLocally();
      },
      error: (err) => this.errorMsg = 'Failed to load reviews: ' + (err.error || err.message)
    });
  }

  filterLocally(): void {
    if (this.searchQuery) {
      const q = this.searchQuery.toLowerCase();
      this.reviews = this.reviews.filter(r => 
        r.reviewTitle.toLowerCase().includes(q) ||
        r.reviewDescription.toLowerCase().includes(q) ||
        r.productName.toLowerCase().includes(q) ||
        r.userFullName.toLowerCase().includes(q)
      );
    }

    if (this.ratingFilter) {
      const ratingVal = parseInt(this.ratingFilter, 10);
      this.reviews = this.reviews.filter(r => r.rating === ratingVal);
    }
  }

  selectReview(review: Review): void {
    this.selectedReview = review;
    this.adminNotes = review.adminNotes || '';
  }

  moderate(status: string): void {
    if (!this.selectedReview) return;
    this.errorMsg = '';

    this.api.moderateReview(this.selectedReview.id, status, this.adminNotes).subscribe({
      next: (updated) => {
        this.showNotice(`Review status set to: ${status}`);
        this.selectedReview = undefined;
        this.loadReviews();
      },
      error: (err) => this.errorMsg = 'Failed to moderate: ' + (err.error || err.message)
    });
  }

  deleteReview(review: Review): void {
    if (!confirm('Are you sure you want to soft delete this review?')) return;
    this.errorMsg = '';

    this.api.deleteReview(review.id).subscribe({
      next: () => {
        this.showNotice('Review soft-deleted successfully.');
        this.selectedReview = undefined;
        this.loadReviews();
      },
      error: (err) => this.errorMsg = 'Failed to delete review: ' + (err.error || err.message)
    });
  }

  restoreReview(review: Review): void {
    this.errorMsg = '';

    this.api.restoreReview(review.id).subscribe({
      next: () => {
        this.showNotice('Review restored successfully.');
        this.loadReviews();
      },
      error: (err) => this.errorMsg = 'Failed to restore review: ' + (err.error || err.message)
    });
  }

  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 3000);
  }
}
