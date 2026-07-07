import { Injectable, signal, inject } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';
import { BreadcrumbItem } from './navigation.models';

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbService {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly breadcrumbs = signal<BreadcrumbItem[]>([]);

  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      const root = this.route.root;
      const crumbs: BreadcrumbItem[] = [];
      this.buildBreadcrumbs(root, '', crumbs);
      this.breadcrumbs.set(crumbs);
    });
  }

  private buildBreadcrumbs(route: ActivatedRoute, url = '', crumbs: BreadcrumbItem[] = []): void {
    const children = route.children;
    if (children.length === 0) return;

    for (const child of children) {
      const routeURL = child.snapshot.url.map(segment => segment.path).join('/');
      let nextUrl = url;
      if (routeURL !== '') {
        nextUrl += `/${routeURL}`;
      }

      // Check if title or breadcrumb metadata is specified
      const label = child.snapshot.data['breadcrumb'] || child.snapshot.data['title'];
      if (label) {
        crumbs.push({ label, route: nextUrl });
      }

      this.buildBreadcrumbs(child, nextUrl, crumbs);
    }
  }
}
