import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ApiService, Transaction } from '../services/api.service';

@Component({
  selector: 'app-transaction-detail',
  imports: [RouterLink, DatePipe, DecimalPipe],
  template: `
    <div class="p-6">
      <a routerLink="/transactions" class="text-blue-400 hover:text-blue-300 text-sm mb-4 inline-block">
        ← Back to Transactions
      </a>

      @if (transaction(); as txn) {
        <div class="bg-gray-800 rounded-lg p-6 space-y-6">
          <div class="flex items-center justify-between">
            <h2 class="text-2xl font-bold text-white">{{ txn.code }}</h2>
            <span
              class="px-3 py-1 rounded text-sm font-medium"
              [class.bg-green-900]="txn.status === 'captured' || txn.status === 'authorized'"
              [class.text-green-300]="txn.status === 'captured' || txn.status === 'authorized'"
              [class.bg-red-900]="txn.status === 'declined' || txn.status === 'failed'"
              [class.text-red-300]="txn.status === 'declined' || txn.status === 'failed'"
              [class.bg-yellow-900]="txn.status === 'refunded'"
              [class.text-yellow-300]="txn.status === 'refunded'"
            >
              {{ txn.status }}
            </span>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <!-- Payment Details -->
            <div class="space-y-3">
              <h3 class="text-lg font-semibold text-gray-300 border-b border-gray-700 pb-2">Payment</h3>
              <div class="space-y-2 text-sm">
                <div class="flex justify-between">
                  <span class="text-gray-400">Amount</span>
                  <span class="text-white font-medium">{{ txn.amount / 100 | number:'1.2-2' }} {{ txn.currencyCode }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-400">Card Brand</span>
                  <span class="text-white">{{ txn.cardBrandCode }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-400">Masked Card</span>
                  <span class="text-white font-mono">{{ txn.maskedCard }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-400">Acquirer</span>
                  <span class="text-white">{{ txn.acquirerCode }}</span>
                </div>
              </div>
            </div>

            <!-- Flight Details -->
            <div class="space-y-3">
              <h3 class="text-lg font-semibold text-gray-300 border-b border-gray-700 pb-2">Flight</h3>
              <div class="space-y-2 text-sm">
                <div class="flex justify-between">
                  <span class="text-gray-400">Airline</span>
                  <span class="text-white">{{ txn.airlineCode }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-400">Flight Number</span>
                  <span class="text-white">{{ txn.flightNumber }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-400">Route</span>
                  <span class="text-white">{{ txn.originAirport }} → {{ txn.destinationAirport }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-400">Passenger</span>
                  <span class="text-white font-mono">{{ txn.passengerReference }}</span>
                </div>
              </div>
            </div>

            <!-- Timeline -->
            <div class="space-y-3">
              <h3 class="text-lg font-semibold text-gray-300 border-b border-gray-700 pb-2">Timeline</h3>
              <div class="space-y-2 text-sm">
                <div class="flex justify-between">
                  <span class="text-gray-400">Transaction Date</span>
                  <span class="text-white">{{ txn.transactionDate | date:'medium' }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-400">Ingested At</span>
                  <span class="text-white">{{ txn.createdAt | date:'medium' }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      } @else {
        <p class="text-gray-400">Loading transaction...</p>
      }
    </div>
  `,
})
export class TransactionDetailComponent implements OnInit {
  transaction = signal<Transaction | null>(null);

  constructor(private route: ActivatedRoute, private api: ApiService) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.api.getTransaction(id).subscribe((txn) => this.transaction.set(txn));
    }
  }
}
