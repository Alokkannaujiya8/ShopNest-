export interface MenuItem {
  label: string;
  route: string;
  icon?: string;
  badge?: string;
  role?: string;
}

export interface BreadcrumbItem {
  label: string;
  route: string;
}
