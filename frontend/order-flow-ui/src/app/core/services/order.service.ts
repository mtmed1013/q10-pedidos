import { inject, Service } from '@angular/core';
import { environment } from '../../../environments/enviroment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order } from '../models/order.model';
import { ApiResponse } from '../models/ApiResponse';
import { CreateOrderRequest } from '../models/create-order-request.model';

@Service()
export class OrderService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/Orders`;

  getAll(): Observable<ApiResponse<Order[]>> {
    return this.http.get<ApiResponse<Order[]>>(this.apiUrl);
  }

  getById(id: string): Observable<ApiResponse<Order>> {
    return this.http.get<ApiResponse<Order>>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateOrderRequest): Observable<ApiResponse<Order>> {
    return this.http.post<ApiResponse<Order>>(this.apiUrl, request);
  }
}
