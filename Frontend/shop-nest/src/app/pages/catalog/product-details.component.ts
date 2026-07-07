import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Product, ProductVariant, Review, RatingSummary, ProductQuestion } from '../../core/api.models';
import { ApiService } from '../../core/api.service';
import { SeoService } from '../../core/seo.service';
import { WishlistService } from '../../core/wishlist.service';

@Component({
  selector: 'app-product-details',
  standalone: false,
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.scss'
})
export class ProductDetailsComponent implements OnInit {
  productSlug = '';
  product?: Product;
  selectedImage = '';
  selectedVariant?: ProductVariant;

  // Selected variant attributes values mapping
  selectedAttributes: Record<string, string> = {};
  availableAttributeNames: string[] = [];

  // Reviews & Q&A
  reviews: Review[] = [];
  ratingSummary?: RatingSummary;
  questions: ProductQuestion[] = [];
  
  showReviewForm = false;
  showQuestionForm = false;
  showReplyFormId = '';

  reviewTitle = '';
  reviewDescription = '';
  ratingValue = 5;
  isRecommended = true;
  reviewImageUrl = '';
  
  questionText = '';
  replyAnswerText = '';

  isAdminOrSeller = false;
  isLoggedIn = false;

  // Page notifications
  notice = '';
  errorMsg = '';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    readonly api: ApiService,
    private readonly seo: SeoService,
    private readonly wishlistService: WishlistService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const slug = params.get('slug');
      const id = params.get('id');
      if (slug) {
        this.productSlug = slug;
        this.loadProduct();
      } else if (id) {
        this.loadProductById(id);
      }
    });
  }

  loadProduct(): void {
    this.api.getProductBySlug(this.productSlug).subscribe({
      next: (p) => this.handleProductLoadSuccess(p),
      error: () => this.errorMsg = 'Product not found or currently unavailable.'
    });
  }

  loadProductById(id: string): void {
    this.api.getProductById(id).subscribe({
      next: (p) => this.handleProductLoadSuccess(p),
      error: () => this.errorMsg = 'Product not found or currently unavailable.'
    });
  }

  private handleProductLoadSuccess(p: Product): void {
    this.product = p;
    this.selectedImage = p.images && p.images.length > 0 ? p.images[0].url : '';
    this.extractAttributes();
    
    this.isLoggedIn = this.api.currentUser() !== null;
    this.isAdminOrSeller = ['Admin', 'Seller'].includes(this.api.currentUser()?.role || '');

    // Set Dynamic SEO Tags
    this.seo.setMetaTags(p.name, p.shortDescription || p.description.substring(0, 150), p.metaKeywords || '');

    this.loadReviewsAndSummary(p.id);
    this.loadQuestions(p.id);

    // Select first variant if any
    if (p.variants && p.variants.length > 0) {
      this.selectVariant(p.variants[0]);
    }
  }

  extractAttributes(): void {
    if (!this.product?.variants) return;
    
    const names = new Set<string>();
    this.product.variants.forEach(v => {
      v.attributeValues.forEach(av => {
        names.add(av.attributeName);
      });
    });
    this.availableAttributeNames = Array.from(names);
  }

  selectVariant(v: ProductVariant): void {
    this.selectedVariant = v;
    if (v.imageUrl) {
      this.selectedImage = v.imageUrl;
    }
    
    // Update attribute selections
    this.selectedAttributes = {};
    v.attributeValues.forEach(av => {
      this.selectedAttributes[av.attributeName] = av.value;
    });
  }

  onAttributeChange(name: string, value: string): void {
    this.selectedAttributes[name] = value;
    this.matchVariant();
  }

  matchVariant(): void {
    if (!this.product?.variants) return;

    const matched = this.product.variants.find(v => {
      return v.attributeValues.every(av => {
        return this.selectedAttributes[av.attributeName] === av.value;
      });
    });

    if (matched) {
      this.selectVariant(matched);
    }
  }

  getValuesForAttribute(name: string): string[] {
    if (!this.product?.variants) return [];
    
    const values = new Set<string>();
    this.product.variants.forEach(v => {
      v.attributeValues.forEach(av => {
        if (av.attributeName === name) {
          values.add(av.value);
        }
      });
    });
    return Array.from(values);
  }

  addToWishlist(): void {
    if (!this.product) return;
    if (!this.api.currentUser()) {
      this.errorMsg = 'Login required before adding items to wishlist.';
      setTimeout(() => this.errorMsg = '', 3000);
      return;
    }

    this.wishlistService.addToWishlist(this.product.id).subscribe({
      next: () => {
        this.notice = 'Added to Wishlist successfully!';
        setTimeout(() => this.notice = '', 3000);
      },
      error: (err) => this.errorMsg = 'Failed to add item to Wishlist: ' + (err.error || err.message)
    });
  }

  addToCart(): void {
    if (!this.product) return;
    
    const targetId = this.selectedVariant ? this.selectedVariant.id : this.product.id;
    this.api.addToCart(targetId, 1).subscribe({
      next: () => {
        this.notice = 'Added to Cart successfully!';
        this.api.refreshCartCount();
        setTimeout(() => this.notice = '', 3000);
      },
      error: () => this.errorMsg = 'Failed to add item to Cart.'
    });
  }

  backToCatalog(): void {
    this.router.navigate(['/catalog']);
  }

  loadReviewsAndSummary(productId: string): void {
    this.api.getProductReviews(productId).subscribe({
      next: (res) => this.reviews = res.items,
      error: (err) => console.error('Failed to load reviews:', err)
    });

    this.api.getRatingSummary(productId).subscribe({
      next: (summary) => this.ratingSummary = summary,
      error: (err) => console.error('Failed to load rating summary:', err)
    });
  }

  loadQuestions(productId: string): void {
    this.api.getProductQuestions(productId).subscribe({
      next: (res) => this.questions = res.items,
      error: (err) => console.error('Failed to load questions:', err)
    });
  }

  submitReview(): void {
    if (!this.product) return;
    this.errorMsg = '';
    
    this.api.addReview({
      productId: this.product.id,
      rating: this.ratingValue,
      reviewTitle: this.reviewTitle,
      reviewDescription: this.reviewDescription,
      isRecommended: this.isRecommended,
      reviewImages: this.reviewImageUrl ? [this.reviewImageUrl] : []
    }).subscribe({
      next: () => {
        this.notice = 'Review submitted successfully! It is pending administrator approval.';
        this.showReviewForm = false;
        this.reviewTitle = '';
        this.reviewDescription = '';
        this.ratingValue = 5;
        this.reviewImageUrl = '';
        this.loadReviewsAndSummary(this.product!.id);
        setTimeout(() => this.notice = '', 4000);
      },
      error: (err) => this.errorMsg = 'Failed to submit review: ' + (err.error || err.message)
    });
  }

  submitQuestion(): void {
    if (!this.product || !this.questionText) return;
    this.errorMsg = '';

    this.api.askQuestion({
      productId: this.product.id,
      questionText: this.questionText
    }).subscribe({
      next: () => {
        this.notice = 'Question asked successfully!';
        this.showQuestionForm = false;
        this.questionText = '';
        this.loadQuestions(this.product!.id);
        setTimeout(() => this.notice = '', 3000);
      },
      error: (err) => this.errorMsg = 'Failed to ask question: ' + (err.error || err.message)
    });
  }

  submitReply(questionId: string): void {
    if (!this.product || !this.replyAnswerText) return;
    this.errorMsg = '';

    this.api.replyToQuestion(questionId, this.replyAnswerText).subscribe({
      next: () => {
        this.notice = 'Reply posted successfully!';
        this.showReplyFormId = '';
        this.replyAnswerText = '';
        this.loadQuestions(this.product!.id);
        setTimeout(() => this.notice = '', 3000);
      },
      error: (err) => this.errorMsg = 'Failed to post reply: ' + (err.error || err.message)
    });
  }

  toggleHelpful(review: Review): void {
    if (!this.isLoggedIn) {
      this.errorMsg = 'You must be logged in to vote on reviews.';
      return;
    }
    
    if (review.hasLiked) {
      this.api.unlikeHelpfulReview(review.id).subscribe({
        next: () => {
          review.hasLiked = false;
          review.helpfulCount = Math.max(0, review.helpfulCount - 1);
        }
      });
    } else {
      this.api.likeHelpfulReview(review.id).subscribe({
        next: () => {
          review.hasLiked = true;
          review.helpfulCount++;
        }
      });
    }
  }

  reportReview(review: Review): void {
    const reason = prompt('Please describe why you are reporting this review:');
    if (!reason) return;

    this.api.reportReview(review.id, reason).subscribe({
      next: () => {
        this.notice = 'Review reported. Thank you for helping keep ShopNest safe!';
        if (this.product) {
          this.loadReviewsAndSummary(this.product.id);
        }
        setTimeout(() => this.notice = '', 3000);
      },
      error: (err) => this.errorMsg = 'Reporting failed: ' + (err.error || err.message)
    });
  }

  deleteReview(reviewId: string): void {
    if (!confirm('Are you sure you want to delete this review?')) return;
    this.api.deleteReview(reviewId).subscribe({
      next: () => {
        this.notice = 'Review deleted successfully.';
        if (this.product) {
          this.loadReviewsAndSummary(this.product.id);
        }
        setTimeout(() => this.notice = '', 3000);
      },
      error: (err) => this.errorMsg = 'Failed to delete: ' + (err.error || err.message)
    });
  }

  deleteQuestion(questionId: string): void {
    if (!confirm('Are you sure you want to delete this question? All replies will be deleted.')) return;
    this.api.deleteQuestion(questionId).subscribe({
      next: () => {
        this.notice = 'Question deleted.';
        if (this.product) {
          this.loadQuestions(this.product.id);
        }
        setTimeout(() => this.notice = '', 3000);
      },
      error: (err) => this.errorMsg = 'Failed to delete question: ' + (err.error || err.message)
    });
  }

  deleteAnswer(answerId: string): void {
    if (!confirm('Are you sure you want to delete this reply?')) return;
    this.api.deleteAnswer(answerId).subscribe({
      next: () => {
        this.notice = 'Reply deleted.';
        if (this.product) {
          this.loadQuestions(this.product.id);
        }
        setTimeout(() => this.notice = '', 3000);
      },
      error: (err) => this.errorMsg = 'Failed to delete reply: ' + (err.error || err.message)
    });
  }
}
