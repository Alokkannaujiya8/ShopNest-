import { Component, Output, EventEmitter, OnInit, OnDestroy, signal } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-search-bar',
  standalone: false,
  template: `
    <div class="search-wrapper">
      <span class="search-icon">🔍</span>
      <input 
        type="text" 
        [value]="query()" 
        (input)="onInputChange($event)"
        placeholder="Search products, brands, or categories..." 
        aria-label="Search items"
      />
      <button 
        type="button" 
        *ngIf="query()" 
        class="clear-btn" 
        (click)="clear()"
        aria-label="Clear search query"
      >
        ×
      </button>
    </div>
  `,
  styles: [`
    .search-wrapper {
      position: relative;
      display: flex;
      align-items: center;
      width: 100%;
      max-width: 480px;
    }
    .search-icon {
      position: absolute;
      left: 12px;
      color: var(--muted);
      font-size: 0.95rem;
    }
    input {
      padding-left: 36px;
      padding-right: 36px;
      font-size: 0.9rem;
    }
    .clear-btn {
      position: absolute;
      right: 10px;
      background: transparent;
      border: 0;
      font-size: 1.3rem;
      cursor: pointer;
      color: var(--muted);
      padding: 0 4px;
      min-height: auto;
    }
  `]
})
export class SearchBarComponent implements OnInit, OnDestroy {
  @Output() readonly search = new EventEmitter<string>();

  readonly query = signal('');
  private readonly searchSubject = new Subject<string>();
  private sub?: Subscription;

  ngOnInit(): void {
    this.sub = this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(val => this.search.emit(val));
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  onInputChange(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.query.set(val);
    this.searchSubject.next(val);
  }

  clear(): void {
    this.query.set('');
    this.searchSubject.next('');
    this.search.emit('');
  }
}
