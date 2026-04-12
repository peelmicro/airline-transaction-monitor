import { Component, OnInit, OnDestroy, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { SignalRService } from '../services/signalr.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, DatePipe, DecimalPipe],
  template: `
    <div class="p-6 space-y-6">
      <!-- Connection status -->
      <div class="flex items-center justify-between">
        <h2 class="text-2xl font-bold text-white">Dashboard</h2>
        <div class="flex items-center space-x-2">
          <span
            class="w-3 h-3 rounded-full"
            [class.bg-green-500]="signalR.connected()"
            [class.bg-red-500]="!signalR.connected()"
          ></span>
          <span class="text-sm text-gray-400">
            {{ signalR.connected() ? 'Live' : 'Disconnected' }}
          </span>
        </div>
      </div>

      <!-- Airline Metrics Cards -->
      <div>
        <h3 class="text-lg font-semibold text-gray-300 mb-3">Airline Metrics</h3>
        @if (airlineGroups().length === 0) {
          <p class="text-gray-500">Waiting for real-time metrics data...</p>
        }
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          @for (group of airlineGroups(); track group.airline) {
            <div class="bg-gray-800 rounded-lg p-4 border border-gray-700">
              <div class="flex items-center justify-between mb-3">
                <h4 class="text-lg font-bold text-white">{{ group.airline }}</h4>
                <span class="text-xs text-gray-500">{{ group.metric.currencyCode }}</span>
              </div>
              <div class="grid grid-cols-2 gap-3 text-sm">
                <div>
                  <span class="text-gray-400">Transactions</span>
                  <p class="text-white font-medium">{{ group.metric.transactionCount }}</p>
                </div>
                <div>
                  <span class="text-gray-400">Volume</span>
                  <p class="text-white font-medium">{{ group.metric.totalVolume / 100 | number:'1.2-2' }}</p>
                </div>
                <div>
                  <span class="text-gray-400">Error Rate</span>
                  <p
                    class="font-medium"
                    [class.text-green-400]="group.metric.errorRate < 5"
                    [class.text-yellow-400]="group.metric.errorRate >= 5 && group.metric.errorRate < 10"
                    [class.text-red-400]="group.metric.errorRate >= 10"
                  >
                    {{ group.metric.errorRate | number:'1.2-2' }}%
                  </p>
                </div>
                <div>
                  <span class="text-gray-400">P95 / P99</span>
                  <p class="text-white font-medium">
                    {{ group.metric.latencyP95Ms }}ms / {{ group.metric.latencyP99Ms }}ms
                  </p>
                </div>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Active Alerts -->
      <div>
        <h3 class="text-lg font-semibold text-gray-300 mb-3">
          Active Alerts
          @if (signalR.alerts().length > 0) {
            <span class="ml-2 bg-red-600 text-white text-xs px-2 py-1 rounded-full">
              {{ signalR.alerts().length }}
            </span>
          }
        </h3>
        @if (signalR.alerts().length === 0) {
          <p class="text-gray-500">No active alerts</p>
        }
        <div class="space-y-2">
          @for (alert of signalR.alerts(); track alert.alertId) {
            <div class="bg-red-900/30 border border-red-800 rounded-lg p-3 flex items-center justify-between">
              <div>
                <span class="text-red-400 font-medium">{{ alert.airlineCode }}</span>
                <span class="text-gray-400 mx-2">—</span>
                <span class="text-gray-300">{{ alert.ruleName }}</span>
              </div>
              <div class="text-right text-sm">
                <p class="text-red-400">{{ alert.actualValue | number:'1.2-2' }}% > {{ alert.threshold }}%</p>
                <p class="text-gray-500">{{ alert.firedAt | date:'HH:mm:ss' }}</p>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Recent Transactions Stream -->
      <div>
        <div class="flex items-center justify-between mb-3">
          <h3 class="text-lg font-semibold text-gray-300">
            Live Transactions
            <span class="text-sm text-gray-500 font-normal ml-2">Latest 50</span>
          </h3>
          <a routerLink="/transactions" class="text-blue-400 hover:text-blue-300 text-sm">View all →</a>
        </div>
        @if (signalR.transactions().length === 0) {
          <p class="text-gray-500">Waiting for real-time transaction data...</p>
        }
        <div class="overflow-x-auto">
          <table class="w-full text-sm text-left">
            <thead class="text-xs text-gray-400 uppercase bg-gray-800">
              <tr>
                <th class="px-3 py-2">Code</th>
                <th class="px-3 py-2">Airline</th>
                <th class="px-3 py-2">Status</th>
                <th class="px-3 py-2">Amount</th>
                <th class="px-3 py-2">Card</th>
                <th class="px-3 py-2">Flight</th>
                <th class="px-3 py-2">Time</th>
              </tr>
            </thead>
            <tbody>
              @for (txn of signalR.transactions(); track txn.transactionId) {
                <tr class="border-b border-gray-700 hover:bg-gray-800/50">
                  <td class="px-3 py-2 text-blue-400">
                    <a [routerLink]="['/transactions', txn.transactionId]">{{ txn.code }}</a>
                  </td>
                  <td class="px-3 py-2 text-white">{{ txn.airlineCode }}</td>
                  <td class="px-3 py-2">
                    <span
                      class="px-2 py-0.5 rounded text-xs font-medium"
                      [class.bg-green-900]="txn.status === 'captured' || txn.status === 'authorized'"
                      [class.text-green-300]="txn.status === 'captured' || txn.status === 'authorized'"
                      [class.bg-red-900]="txn.status === 'declined' || txn.status === 'failed'"
                      [class.text-red-300]="txn.status === 'declined' || txn.status === 'failed'"
                      [class.bg-yellow-900]="txn.status === 'refunded'"
                      [class.text-yellow-300]="txn.status === 'refunded'"
                    >
                      {{ txn.status }}
                    </span>
                  </td>
                  <td class="px-3 py-2 text-white">{{ txn.amount / 100 | number:'1.2-2' }} {{ txn.currencyCode }}</td>
                  <td class="px-3 py-2 text-gray-400">{{ txn.maskedCard }}</td>
                  <td class="px-3 py-2 text-gray-400">{{ txn.flightNumber }}</td>
                  <td class="px-3 py-2 text-gray-400">{{ txn.createdAt | date:'HH:mm:ss' }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
})
export class DashboardComponent implements OnInit, OnDestroy {
  constructor(
    private auth: AuthService,
    public signalR: SignalRService
  ) {}

  // Group metrics by airline (show only 5-minute window)
  airlineGroups = computed(() => {
    const metrics = this.signalR.metrics();
    const fiveMinMetrics = metrics.filter((m) => m.windowMinutes === 5);
    return fiveMinMetrics.map((m) => ({ airline: m.airlineCode, metric: m }));
  });

  ngOnInit() {
    const token = this.auth.token();
    if (token) {
      this.signalR.start(token);
    }
  }

  ngOnDestroy() {
    this.signalR.stop();
  }
}
