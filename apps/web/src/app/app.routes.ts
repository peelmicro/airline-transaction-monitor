import { Routes } from '@angular/router';
import { authGuard } from './services/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./auth/login').then((m) => m.LoginComponent),
  },
  {
    path: '',
    loadComponent: () => import('./shared/layout').then((m) => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./dashboard/dashboard').then((m) => m.DashboardComponent),
      },
      {
        path: 'transactions',
        loadComponent: () =>
          import('./transactions/transactions').then(
            (m) => m.TransactionsComponent
          ),
      },
      {
        path: 'transactions/:id',
        loadComponent: () =>
          import('./transactions/transaction-detail').then(
            (m) => m.TransactionDetailComponent
          ),
      },
      {
        path: 'alerts',
        loadComponent: () =>
          import('./alerts/alerts').then((m) => m.AlertsComponent),
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
];
