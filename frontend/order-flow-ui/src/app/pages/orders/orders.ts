import { ToastModule } from 'primeng/toast';
import {
  Component,
  OnInit,
  inject,
  signal,
  ChangeDetectionStrategy,
  viewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { MultiSelectModule } from 'primeng/multiselect';
import { SliderModule } from 'primeng/slider';
import { Table, TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FilterSlash } from '@primeicons/angular/filter-slash';
import { Search } from '@primeicons/angular/search';
import { OrderService } from '../../core/services/order.service';
import { MessageService } from 'primeng/api';
import { catchError, finalize, interval, of, Subject, takeUntil } from 'rxjs';
import { Order } from '../../core/models/order.model';
import { DatePipe } from '@angular/common';
import { ModalCreateOrder } from '../../components/modal-create-order/modal-create-order';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-orders',
  imports: [
    SelectModule,
    IconFieldModule,
    InputIconModule,
    MultiSelectModule,
    SliderModule,
    TableModule,
    TagModule,
    ButtonModule,
    InputTextModule,
    FormsModule,
    Search,
    ToastModule,
    DatePipe,
    ModalCreateOrder,
    CardModule
  ],
  templateUrl: './orders.html',
  styleUrl: './orders.scss',
})
export class Orders implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly messageService = inject(MessageService);
  modal = viewChild.required(ModalCreateOrder);

  orders = signal<Order[]>([]);
  statuses = signal<any[]>([]);
  loading = signal(true);
  searchValue: string = '';

  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loading.set(true);
    this.loadOrders();
    interval(5000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadOrders();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  clear(table: Table) {
    table.clear();
    this.searchValue = '';
  }

  loadOrders(): void {
    this.orderService
      .getAll()
      .pipe(
        catchError((error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.message ?? 'Ocurrio un error inesperado',
          });

          return of(null);
        }),
        finalize(() => {
          this.loading.set(false);
        }),
      )
      .subscribe((response) => {
        if (!response) {
          return;
        }

        if (response.success) {
          this.orders.set(response.data);
        }
      });
  }

  getSeverity(status: string) {
    switch (status) {
      case 'Confirmed':
        return 'success';

      case 'Rejected':
        return 'danger';

      default:
        return 'warn';
    }
  }
}
