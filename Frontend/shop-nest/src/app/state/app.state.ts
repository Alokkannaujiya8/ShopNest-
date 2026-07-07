import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action, createAction, createReducer, createSelector, on, props } from '@ngrx/store';
import { catchError, map, mergeMap, of, tap } from 'rxjs';
import { AuthResponse, Cart, Category, PagedResult, Product } from '../core/api.models';
import { ApiService } from '../core/api.service';

// ==========================================
// 1. STATE DEFINITIONS
// ==========================================

export interface AuthState {
  user: AuthResponse | null;
  loading: boolean;
  error: string | null;
}

export interface CartState {
  cart: Cart | null;
  loading: boolean;
  error: string | null;
}

export interface ProductsState {
  productsResult: PagedResult<Product> | null;
  categories: Category[];
  loading: boolean;
  error: string | null;
}

export interface AppState {
  auth: AuthState;
  cart: CartState;
  products: ProductsState;
}

const initialAuthState: AuthState = {
  user: readSession(),
  loading: false,
  error: null,
};

const initialCartState: CartState = {
  cart: null,
  loading: false,
  error: null,
};

const initialProductsState: ProductsState = {
  productsResult: null,
  categories: [],
  loading: false,
  error: null,
};

function readSession(): AuthResponse | null {
  const raw = localStorage.getItem('shopnest.session');
  return raw ? (JSON.parse(raw) as AuthResponse) : null;
}

// ==========================================
// 2. ACTIONS
// ==========================================

// Auth Actions
export const login = createAction('[Auth] Login', props<{ email: string; password: string }>());
export const loginSuccess = createAction('[Auth] Login Success', props<{ user: AuthResponse }>());
export const loginFailure = createAction('[Auth] Login Failure', props<{ error: string }>());

export const registerUser = createAction('[Auth] Register', props<{ fullName: string; email: string; password: string; role: string }>());
export const registerSuccess = createAction('[Auth] Register Success', props<{ user: AuthResponse }>());
export const registerFailure = createAction('[Auth] Register Failure', props<{ error: string }>());

export const logout = createAction('[Auth] Logout');

// Cart Actions
export const loadCart = createAction('[Cart] Load Cart');
export const loadCartSuccess = createAction('[Cart] Load Cart Success', props<{ cart: Cart }>());
export const loadCartFailure = createAction('[Cart] Load Cart Failure', props<{ error: string }>());

export const addToCart = createAction('[Cart] Add To Cart', props<{ productId: string; quantity: number }>());
export const updateCartItem = createAction('[Cart] Update Cart Item', props<{ itemId: string; quantity: number }>());
export const removeCartItem = createAction('[Cart] Remove Cart Item', props<{ itemId: string }>());

// Products Actions
export const loadProducts = createAction('[Products] Load Products', props<{ filters: Record<string, any> }>());
export const loadProductsSuccess = createAction('[Products] Load Products Success', props<{ result: PagedResult<Product> }>());
export const loadProductsFailure = createAction('[Products] Load Products Failure', props<{ error: string }>());

export const loadCategories = createAction('[Products] Load Categories');
export const loadCategoriesSuccess = createAction('[Products] Load Categories Success', props<{ categories: Category[] }>());

// ==========================================
// 3. REDUCERS
// ==========================================

export const authReducer = createReducer(
  initialAuthState,
  on(login, (state) => ({ ...state, loading: true, error: null })),
  on(loginSuccess, (state, { user }) => ({ ...state, user, loading: false, error: null })),
  on(loginFailure, (state, { error }) => ({ ...state, loading: false, error })),
  on(registerUser, (state) => ({ ...state, loading: true, error: null })),
  on(registerSuccess, (state, { user }) => ({ ...state, user, loading: false, error: null })),
  on(registerFailure, (state, { error }) => ({ ...state, loading: false, error })),
  on(logout, (state) => ({ ...state, user: null, error: null }))
);

export const cartReducer = createReducer(
  initialCartState,
  on(loadCart, (state) => ({ ...state, loading: true, error: null })),
  on(loadCartSuccess, (state, { cart }) => ({ ...state, cart, loading: false, error: null })),
  on(loadCartFailure, (state, { error }) => ({ ...state, loading: false, error })),
  on(logout, () => initialCartState)
);

export const productsReducer = createReducer(
  initialProductsState,
  on(loadProducts, (state) => ({ ...state, loading: true, error: null })),
  on(loadProductsSuccess, (state, { result }) => ({ ...state, productsResult: result, loading: false, error: null })),
  on(loadProductsFailure, (state, { error }) => ({ ...state, loading: false, error })),
  on(loadCategoriesSuccess, (state, { categories }) => ({ ...state, categories }))
);

// ==========================================
// 4. SELECTORS
// ==========================================

export const selectAuth = (state: AppState) => state.auth;
export const selectCurrentUser = createSelector(selectAuth, (state) => state.user);
export const selectAuthLoading = createSelector(selectAuth, (state) => state.loading);
export const selectAuthError = createSelector(selectAuth, (state) => state.error);

export const selectCartState = (state: AppState) => state.cart;
export const selectCart = createSelector(selectCartState, (state) => state.cart);
export const selectCartLoading = createSelector(selectCartState, (state) => state.loading);

export const selectProductsState = (state: AppState) => state.products;
export const selectProductsResult = createSelector(selectProductsState, (state) => state.productsResult);
export const selectCategories = createSelector(selectProductsState, (state) => state.categories);
export const selectProductsLoading = createSelector(selectProductsState, (state) => state.loading);

// ==========================================
// 5. EFFECTS
// ==========================================

@Injectable()
export class AuthEffects {
  private readonly actions$ = inject(Actions);
  private readonly api = inject(ApiService);

  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(login),
      mergeMap(({ email, password }) =>
        this.api.login(email, password).pipe(
          map((res) => loginSuccess({ user: res.data! })),
          catchError((err) => of(loginFailure({ error: err.error?.errors?.[0] || 'Login failed.' })))
        )
      )
    )
  );

  register$ = createEffect(() =>
    this.actions$.pipe(
      ofType(registerUser),
      mergeMap(({ fullName, email, password, role }) =>
        this.api.register({ fullName, email, mobileNumber: '', password, confirmPassword: password, acceptTerms: true, role }).pipe(
          map(() => registerSuccess({ user: null as any })),
          catchError((err) => of(registerFailure({ error: err.error?.errors?.[0] || 'Registration failed.' })))
        )
      )
    )
  );

  logout$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(logout),
        tap(() => this.api.logout())
      ),
    { dispatch: false }
  );
}

@Injectable()
export class CartEffects {
  private readonly actions$ = inject(Actions);
  private readonly api = inject(ApiService);

  loadCart$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadCart),
      mergeMap(() =>
        this.api.cart().pipe(
          map((cart) => loadCartSuccess({ cart })),
          catchError((err) => of(loadCartFailure({ error: err.error?.error || 'Failed to load cart.' })))
        )
      )
    )
  );

  addToCart$ = createEffect(() =>
    this.actions$.pipe(
      ofType(addToCart),
      mergeMap(({ productId, quantity }) =>
        this.api.addToCart(productId, quantity).pipe(
          map((cart) => loadCartSuccess({ cart })),
          catchError((err) => of(loadCartFailure({ error: err.error?.error || 'Failed to add item.' })))
        )
      )
    )
  );

  updateCartItem$ = createEffect(() =>
    this.actions$.pipe(
      ofType(updateCartItem),
      mergeMap(({ itemId, quantity }) =>
        this.api.updateCartItem(itemId, quantity).pipe(
          map((cart) => loadCartSuccess({ cart })),
          catchError((err) => of(loadCartFailure({ error: err.error?.error || 'Failed to update item.' })))
        )
      )
    )
  );

  removeCartItem$ = createEffect(() =>
    this.actions$.pipe(
      ofType(removeCartItem),
      mergeMap(({ itemId }) =>
        this.api.removeCartItem(itemId).pipe(
          map((cart) => loadCartSuccess({ cart })),
          catchError((err) => of(loadCartFailure({ error: err.error?.error || 'Failed to remove item.' })))
        )
      )
    )
  );
}

@Injectable()
export class ProductsEffects {
  private readonly actions$ = inject(Actions);
  private readonly api = inject(ApiService);

  loadProducts$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadProducts),
      mergeMap(({ filters }) =>
        this.api.products(filters).pipe(
          map((result) => loadProductsSuccess({ result })),
          catchError((err) => of(loadProductsFailure({ error: err.error?.error || 'Failed to load products.' })))
        )
      )
    )
  );

  loadCategories$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadCategories),
      mergeMap(() =>
        this.api.categories().pipe(
          map((categories) => loadCategoriesSuccess({ categories })),
          catchError(() => of(loadCategoriesSuccess({ categories: [] })))
        )
      )
    )
  );
}
