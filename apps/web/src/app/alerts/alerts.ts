import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ApiService, Alert } from '../services/api.service';

@Component({
  selector: 'app-alerts',
  imports: [FormsModule, DatePipe, DecimalPipe],
  template: `
    <div class="p-6 space-y-4">
      <h2 class="text-2xl font-bold text-white">Alert History</h2>

      <!-- Filters -->
      <div class="bg-gray-800 rounded-lg p-4 flex flex-wrap gap-3">
        <div>
          <label class="block text-xs text-gray-400 mb-1">Airline</label>
          <select [(ngModel)]="airlineFilter" (ngModelChange)="loadAlerts()"
            class="bg-gray-700 text-white text-sm rounded px-2 py-1.5 border border-gray-600">
            <option value="">All</option>
            <option value="Ryanair">Ryanair</option>
            <option value="Iberia">Iberia</option>
            <option value="BritishAirways">British Airways</option>
            <option value="EasyJet">EasyJet</option>
            <option value="AmericanAirlines">American Airlines</option>
            <option value="DeltaAirLines">Delta Air Lines</option>
          </select>
        </div>
        <div>
          <label class="block text-xs text-gray-400 mb-1">Status</label>
          <select [(ngModel)]="statusFilter" (ngModelChange)="loadAlerts()"
            class="bg-gray-700 text-white text-sm rounded px-2 py-1.5 border border-gray-600">
            <option value="">All</option>
            <option value="active">Active</option>
            <option value="resolved">Resolved</option>
          </select>
        </div>
      </div>

      <!-- Results -->
      <p class="text-sm text-gray-400">
        {{ totalCount() }} alerts found — Page {{ page }} of {{ totalPages() }}
      </p>

      <div class="overflow-x-auto">
        <table class="w-full text-sm text-left">
          <thead class="text-xs text-gray-400 uppercase bg-gray-800">
            <tr>
              <th class="px-3 py-2">Code</th>
              <th class="px-3 py-2">Airline</th>
              <th class="px-3 py-2">Rule</th>
              <th class="px-3 py-2">Window</th>
              <th class="px-3 py-2">Threshold</th>
              <th class="px-3 py-2">Actual</th>
              <th class="px-3 py-2">Status</th>
              <th class="px-3 py-2">Fired At</th>
              <th class="px-3 py-2">Resolved At</th>
            </tr>
          </thead>
          <tbody>
            @for (alert of alerts(); track alert.id) {
              <tr class="border-b border-gray-700 hover:bg-gray-800/50">
                <td class="px-3 py-2 text-blue-400">{{ alert.code }}</td>
                <td class="px-3 py-2 text-white">{{ alert.airlineCode }}</td>
                <td class="px-3 py-2 text-gray-300">{{ alert.ruleName }}</td>
                <td class="px-3 py-2 text-gray-400">{{ alert.windowMinutes }}m</td>
                <td class="px-3 py-2 text-gray-400">{{ alert.threshold | number:'1.2-2' }}%</td>
                <td class="px-3 py-2 text-red-400 font-medium">{{ alert.actualValue | number:'1.2-2' }}%</td>
                <td class="px-3 py-2">
                  <span
                    class="px-2 py-0.5 rounded text-xs font-medium"
                    [class.bg-red-900]="alert.status === 'active'"
                    [class.text-red-300]="alert.status === 'active'"
                    [class.bg-green-900]="alert.status === 'resolved'"
                    [class.text-green-300]="alert.status === 'resolved'"
                  >
                    {{ alert.status }}
                  </span>
                </td>
                <td class="px-3 py-2 text-gray-400">{{ alert.firedAt | date:'short' }}</td>
                <td class="px-3 py-2 text-gray-400">{{ alert.resolvedAt ? (alert.resolvedAt | date:'short') : '—' }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      @if (totalPages() > 1) {
        <div class="flex justify-center space-x-2">
          <button (click)="goToPage(page - 1)" [disabled]="page === 1"
            class="px-3 py-1 bg-gray-700 text-gray-300 rounded disabled:opacity-50 cursor-pointer disabled:cursor-default">
            Previous
          </button>
          <span class="px-3 py-1 text-gray-400">Page {{ page }} of {{ totalPages() }}</span>
          <button (click)="goToPage(page + 1)" [disabled]="page === totalPages()"
            class="px-3 py-1 bg-gray-700 text-gray-300 rounded disabled:opacity-50 cursor-pointer disabled:cursor-default">
            Next
          </button>
        </div>
      }
    </div>
  `,
})
export class AlertsComponent implements OnInit {
  alerts = signal<Alert[]>([]);
  totalCount = signal(0);
  totalPages = signal(1);

  airlineFilter = '';
  statusFilter = '';
  page = 1;
  pageSize = 20;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadAlerts();
  }

  loadAlerts() {
    this.api
      .getAlerts(this.airlineFilter || undefined, this.statusFilter || undefined, this.page, this.pageSize)
      .subscribe((res) => {
        this.alerts.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(Math.ceil(res.totalCount / this.pageSize));
      });
  }

  goToPage(page: number) {
    this.page = page;
    this.loadAlerts();
  }
}
