import { Routes } from '@angular/router';
import { Orders } from './pages/orders/orders';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'orders',
    pathMatch: 'full'
  },
  {
    path: 'orders',
    component: Orders
  },
  {
    path: '**',
    redirectTo: 'orders'
  }
];