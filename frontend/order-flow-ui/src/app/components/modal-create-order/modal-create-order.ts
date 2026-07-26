import { Component, EventEmitter, Output, inject, signal, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';

import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';

import { OrderService } from '../../core/services/order.service';
import { StockService } from '../../core/services/stock.service';

import { MessageService } from 'primeng/api';
import { catchError, exhaustMap, of, Subject } from 'rxjs';
import { CreateOrderRequest } from '../../core/models/create-order-request.model';
import { ListBoxDto } from '../../core/models/listbox.model';

@Component({
  selector: 'app-modal-create-order',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    ButtonModule,
    SelectModule,
  ],
  templateUrl: './modal-create-order.html',
  styleUrl: './modal-create-order.scss',
})
export class ModalCreateOrder implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly orderService = inject(OrderService);
  private readonly stockService = inject(StockService);
  private readonly messageService = inject(MessageService);

  @Output()
  saved = new EventEmitter<void>();

  visible = signal(false);
  stockItems = signal<ListBoxDto[]>([]);

  private readonly submit$ = new Subject<CreateOrderRequest>();

  form = this.fb.group({
    clienteNombre: ['', [Validators.required]],
    sku: ['', [Validators.required]],
    cantidad: [1, [Validators.required, Validators.min(1)]],
  });

  constructor() {
    this.submit$
      .pipe(
        exhaustMap((data) =>
          this.orderService.create(data).pipe(
            catchError((error) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: error?.error?.message ?? 'Error creando pedido',
              });
              return of(null);
            }),
          ),
        ),
      )
      .subscribe((response) => {
        if (!response) {
          return;
        }
        if (response.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Correcto',
            detail: response.message,
          });

          this.visible.set(false);

          this.form.reset({
            cantidad: 1,
          });
          this.saved.emit();
        }
      });
  }

  ngOnInit(): void {
    this.stockService
      .getStockItems()
      .pipe(
        catchError((error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error?.error?.message ?? 'Error al cargar SKUs',
          });
          return of(null);
        }),
      )
      .subscribe((items) => {
        if (items) {
          this.stockItems.set(items.data);
        }
      });
  }

  hasError(field: string): boolean {
    const control = this.form.get(field);
    return !!(control && control.invalid && control.touched);
  }

  open(): void {
    this.visible.set(true);
  }

  close(): void {
    this.visible.set(false);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submit$.next(this.form.value as CreateOrderRequest);
  }
}
