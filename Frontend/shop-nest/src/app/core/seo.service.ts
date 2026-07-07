import { Injectable, inject } from '@angular/core';
import { Title, Meta } from '@angular/platform-browser';

@Injectable({
  providedIn: 'root'
})
export class SeoService {
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);

  setMetaTags(title: string, description: string, keywords: string = ''): void {
    this.title.setTitle(`${title} | ShopNest`);
    
    this.meta.updateTag({ name: 'description', content: description });
    if (keywords) {
      this.meta.updateTag({ name: 'keywords', content: keywords });
    }
    
    // Open Graph / Social Media sharing tags
    this.meta.updateTag({ property: 'og:title', content: `${title} | ShopNest` });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
    this.meta.updateTag({ property: 'og:site_name', content: 'ShopNest E-Commerce' });
  }
}
