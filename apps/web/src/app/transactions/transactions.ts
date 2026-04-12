import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ApiService, Transaction, TransactionFilter } from '../services/api.service';

@Component({
  selector: 'app-transactions',
  imports: [RouterLink, FormsModule, DatePipe, DecimalPipe],
  template: `
    <div class="p-6 space-y-4">
      <h2 class="text-2xl font-bold text-white">Transactions</h2>

      <!-- Filters -->
      <div class="bg-gray-800 rounded-lg p-4 grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-3">
        <div>
          <label class="block text-xs text-gray-400 mb-1">Airline</label>
          <select [(ngModel)]="filter.airlineCode" (ngModelChange)="loadTransactions()"
            class="w-full bg-gray-700 text-white text-sm rounded px-2 py-1.5 border border-gray-600">
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
          <select [(ngModel)]="filter.status" (ngModelChange)="loadTransactions()"
            class="w-full bg-gray-700 text-white text-sm rounded px-2 py-1.5 border border-gray-600">
            <option value="">All</option>
            <option value="authorized">Authorized</option>
            <option value="captured">Captured</option>
            <option value="declined">Declined</option>
            <option value="failed">Failed</option>
            <option value="refunded">Refunded</option>
          </select>
        </div>
        <div>
          <label class="block text-xs text-gray-400 mb-1">Card Brand</label>
          <select [(ngModel)]="filter.cardBrandCode" (ngModelChange)="loadTransactions()"
            class="w-full bg-gray-700 text-white text-sm rounded px-2 py-1.5 border border-gray-600">
            <option value="">All</option>
            <option value="Visa">Visa</option>
            <option value="Mastercard">Mastercard</option>
            <option value="Amex">Amex</option>
            <option value="UnionPay">UnionPay</option>
            <option value="JCB">JCB</option>
          </select>
        </div>
        <div>
          <label class="block text-xs text-gray-400 mb-1">Acquirer</label>
          <select [(ngModel)]="filter.acquirerCode" (ngModelChange)="loadTransactions()"
            class="w-full bg-gray-700 text-white text-sm rounded px-2 py-1.5 border border-gray-600">
            <option value="">All</option>
            <option value="Adyen">Adyen</option>
            <option value="Worldpay">Worldpay</option>
            <option value="ElavonUS">Elavon US</option>
            <option value="ElavonEU">Elavon EU</option>
            <option value="Barclays">Barclays</option>
            <option value="Santander">Santander</option>
          </select>
        </div>
        <div>
          <label class="block text-xs text-gray-400 mb-1">From Date</label>
          <input type="date" [(ngModel)]="filter.fromDate" (ngModelChange)="loadTransactions()"
            class="w-full bg-gray-700 text-white text-sm rounded px-2 py-1.5 border border-gray-600" />
        </div>
        <div>
          <label class="block text-xs text-gray-400 mb-1">To Date</label>
          <input type="date" [(ngModel)]="filter.toDate" (ngModelChange)="loadTransactions()"
            class="w-full bg-gray-700 text-white text-sm rounded px-2 py-1.5 border border-gray-600" />
        </div>
      </div>

      <!-- Results count -->
      <p class="text-sm text-gray-400">
        {{ totalCount() }} transactions found — Page {{ filter.page }} of {{ totalPages() }}
      </p>

      <!-- Table -->
      <div class="overflow-x-auto">
        <table class="w-full text-sm text-left">
          <thead class="text-xs text-gray-400 uppercase bg-gray-800">
            <tr>
              <th class="px-3 py-2">Code</th>
              <th class="px-3 py-2">Airline</th>
              <th class="px-3 py-2">Status</th>
              <th class="px-3 py-2">Amount</th>
              <th class="px-3 py-2">Card Brand</th>
              <th class="px-3 py-2">Masked Card</th>
              <th class="px-3 py-2">Acquirer</th>
              <th class="px-3 py-2">Flight</th>
              <th class="px-3 py-2">Route</th>
              <th class="px-3 py-2">Date</th>
            </tr>
          </thead>
          <tbody>
            @for (txn of transactions(); track txn.id) {
              <tr class="border-b border-gray-700 hover:bg-gray-800/50 cursor-pointer"
                  [routerLink]="['/transactions', txn.id]">
                <td class="px-3 py-2 text-blue-400">{{ txn.code }}</td>
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
                <td class="px-3 py-2 text-gray-300">{{ txn.cardBrandCode }}</td>
                <td class="px-3 py-2 text-gray-400 font-mono text-xs">{{ txn.maskedCard }}</td>
                <td class="px-3 py-2 text-gray-300">{{ txn.acquirerCode }}</td>
                <td class="px-3 py-2 text-gray-400">{{ txn.flightNumber }}</td>
                <td class="px-3 py-2 text-gray-400">{{ txn.originAirport }} → {{ txn.destinationAirport }}</td>
                <td class="px-3 py-2 text-gray-400">{{ txn.transactionDate | date:'short' }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      @if (totalPages() > 1) {
        <div class="flex justify-center space-x-2">
          <button (click)="goToPage(filter.page! - 1)" [disabled]="filter.page === 1"
            class="px-3 py-1 bg-gray-700 text-gray-300 rounded disabled:opacity-50 cursor-pointer disabled:cursor-default">
            Previous
          </button>
          <span class="px-3 py-1 text-gray-400">Page {{ filter.page }} of {{ totalPages() }}</span>
          <button (click)="goToPage(filter.page! + 1)" [disabled]="filter.page === totalPages()"
            class="px-3 py-1 bg-gray-700 text-gray-300 rounded disabled:opacity-50 cursor-pointer disabled:cursor-default">
            Next
          </button>
        </div>
      }
    </div>
  `,
})
export class TransactionsComponent implements OnInit {
  transactions = signal<Transaction[]>([]);
  totalCount = signal(0);
  totalPages = signal(1);

  filter: TransactionFilter = {
    page: 1,
    pageSize: 20,
  };

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadTransactions();
  }

  loadTransactions() {
    this.api.getTransactions(this.filter).subscribe((res) => {
      this.transactions.set(res.items);
      this.totalCount.set(res.totalCount);
      this.totalPages.set(Math.ceil(res.totalCount / (this.filter.pageSize || 20)));
    });
  }

  goToPage(page: number) {
    this.filter.page = page;
    this.loadTransactions();
  }
}
