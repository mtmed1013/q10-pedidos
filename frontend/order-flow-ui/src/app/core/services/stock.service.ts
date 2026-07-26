import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/enviroment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ListBoxDto } from '../models/listbox.model';
import { ApiResponse } from '../models/ApiResponse';

@Injectable({
  providedIn: 'root',
})
export class StockService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/Stock`;

  getStockItems(): Observable<ApiResponse<ListBoxDto[]>> {
    return this.http
      .get<ApiResponse<ListBoxDto[]>>(`${this.apiUrl}/list`);
  }
}
