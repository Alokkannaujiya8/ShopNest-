import { Component, OnInit, inject, signal } from '@angular/core';
import { HomeService, HomeData } from '../../core/home.service';
import { ApiService } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { Category, Product } from '../../core/api.models';

@Component({
  selector: 'app-catalog-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  private readonly homeService = inject(HomeService);
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly error = signal(false);

  readonly categories = signal<Category[]>([]);
  readonly featuredProducts = signal<Product[]>([]);
  readonly newArrivals = signal<Product[]>([]);
  readonly bestSellers = signal<Product[]>([]);

  // Hero carousel active slide tracker
  readonly currentSlide = signal(0);
  readonly heroSlides = [
    {
      title: 'Summer Fashion Arrivals',
      subtitle: 'Up to 50% off on all premium clothing collections',
      image: 'https://images.unsplash.com/photo-1483985988355-763728e1935b?auto=format&fit=crop&w=1200&q=80',
      route: '/catalog'
    },
    {
      title: 'Next-Gen Smart Electronics',
      subtitle: 'Experience innovation with our wireless gadgets',
      image: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=1200&q=80',
      route: '/catalog'
    },
    {
      title: 'Home & Kitchen Makeover',
      subtitle: 'Minimalist decor for modern spaces',
      image: 'https://images.unsplash.com/photo-1513694203232-719a280e022f?auto=format&fit=crop&w=1200&q=80',
      route: '/catalog'
    }
  ];

  // Testimonials lists
  readonly testimonials = [
    { name: 'Sarah Miller', feedback: 'ShopNest transformed my online shopping. Delivery was incredibly fast, and the packaging was absolutely premium!', rating: 5 },
    { name: 'David Chen', feedback: 'The customer service team resolved my sizing inquiry within minutes. Excellent product quality!', rating: 5 },
    { name: 'Emma Watson', feedback: 'Stunning collection! Every item feels authentic and tailored. Will definitely purchase again.', rating: 5 }
  ];

  // FAQ accordion questions
  readonly faqs = [
    { question: 'What is the standard delivery timeframe?', answer: 'We deliver all metropolitan orders within 2 to 3 business days. Regional shipments can take up to 5 business days.', open: signal(false) },
    { question: 'What is your refund policy?', answer: 'We offer a hassle-free 30-day return policy for all unused products in their original packaging.', open: signal(false) },
    { question: 'How can I track my order?', answer: 'You can view real-time delivery status updates by visiting the Orders History page in your customer profile.', open: signal(false) }
  ];

  // Mock brands lists
  readonly brands = [
    { name: 'UrbanStyle', logo: '🏷️' },
    { name: 'AeroTech', logo: '⚡' },
    { name: 'EcoLiving', logo: '🌿' },
    { name: 'LuxeGlow', logo: '✨' }
  ];

  readonly newsletterEmail = signal('');

  ngOnInit(): void {
    this.loadHomeData();
  }

  loadHomeData(): void {
    this.loading.set(true);
    this.error.set(false);
    this.homeService.getHomeData().subscribe({
      next: (data) => {
        this.categories.set(data.categories);
        this.featuredProducts.set(data.featuredProducts);
        this.newArrivals.set(data.newArrivals);
        this.bestSellers.set(data.bestSellers);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set(true);
        this.toast.error('Failed to load store catalog data.');
      }
    });
  }

  nextSlide(): void {
    this.currentSlide.update(i => (i + 1) % this.heroSlides.length);
  }

  prevSlide(): void {
    this.currentSlide.update(i => (i - 1 + this.heroSlides.length) % this.heroSlides.length);
  }

  toggleFaq(index: number): void {
    this.faqs[index].open.update(o => !o);
  }

  subscribeNewsletter(): void {
    const email = this.newsletterEmail().trim();
    if (!email || !email.includes('@')) {
      this.toast.warning('Please enter a valid email address.');
      return;
    }

    this.toast.success('Thank you! You have been successfully subscribed to our newsletter.');
    this.newsletterEmail.set('');
  }

  toggleWishlist(product: Product, event: MouseEvent): void {
    event.stopPropagation();
    event.preventDefault();
    
    if (!this.api.currentUser()) {
      this.toast.warning('Please log in to manage your wishlist.');
      return;
    }

    this.api.addToWishlist(product.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.toast.success(`"${product.name}" added to your wishlist.`);
        }
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to update wishlist.';
        this.toast.error(msg);
      }
    });
  }

  addToCart(product: Product, event: MouseEvent): void {
    event.stopPropagation();
    event.preventDefault();

    if (!this.api.currentUser()) {
      this.toast.warning('Please log in to add items to your cart.');
      return;
    }

    this.api.addToCart(product.id, 1).subscribe({
      next: () => {
        this.toast.success(`"${product.name}" added to cart!`);
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to add item to cart.';
        this.toast.error(msg);
      }
    });
  }
}
