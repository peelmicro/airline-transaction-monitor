import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Transaction {
  id: string;
  code: string;
  maskedCard: string;
  airlineCode: string;
  acquirerCode: string;
  cardBrandCode: string;
  amount: number;
  currencyCode: string;
  status: string;
  transactionDate: string;
  flightNumber: string;
  originAirport: string;
  destinationAirport: string;
  passengerReference: string;
  createdAt: string;
}

export interface TransactionListResponse {
  items: Transaction[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface AirlineMetric {
  airlineCode: string;
  windowMinutes: number;
  transactionCount: number;
  totalVolume: number;
  currencyCode: string;
  errorCount: number;
  errorRate: number;
  latencyP95Ms: number;
  latencyP99Ms: number;
  updatedAt: string;
}

export interface Alert {
  id: string;
  code: string;
  airlineCode: string;
  ruleName: string;
  windowMinutes: number;
  threshold: number;
  actualValue: number;
  status: string;
  firedAt: string;
  resolvedAt: string | null;
}

export interface AlertListResponse {
  items: Alert[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface TransactionFilter {
  airlineCode?: string;
  acquirerCode?: string;
  cardBrandCode?: string;
  status?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getTransactions(filter: TransactionFilter = {}): Observable<TransactionListResponse> {
    let params = new HttpParams();
    if (filter.airlineCode) params = params.set('airlineCode', filter.airlineCode);
    if (filter.acquirerCode) params = params.set('acquirerCode', filter.acquirerCode);
    if (filter.cardBrandCode) params = params.set('cardBrandCode', filter.cardBrandCode);
    if (filter.status) params = params.set('status', filter.status);
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<TransactionListResponse>(`${this.baseUrl}/api/transactions`, { params });
  }

  getTransaction(id: string): Observable<Transaction> {
    return this.http.get<Transaction>(`${this.baseUrl}/api/transactions/${id}`);
  }

  getAirlineMetrics(code: string): Observable<AirlineMetric[]> {
    return this.http.get<AirlineMetric[]>(`${this.baseUrl}/api/airlines/${code}/metrics`);
  }

  getAlerts(airlineCode?: string, status?: string, page = 1, pageSize = 50): Observable<AlertListResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (airlineCode) params = params.set('airlineCode', airlineCode);
    if (status) params = params.set('status', status);

    return this.http.get<AlertListResponse>(`${this.baseUrl}/api/alerts`, { params });
  }
}
