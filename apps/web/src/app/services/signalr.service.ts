import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';

export interface TransactionEvent {
  transactionId: string;
  code: string;
  airlineCode: string;
  acquirerCode: string;
  cardBrandCode: string;
  maskedCard: string;
  amount: number;
  currencyCode: string;
  status: string;
  flightNumber: string;
  originAirport: string;
  destinationAirport: string;
  passengerReference: string;
  transactionDate: string;
  createdAt: string;
}

export interface MetricsEvent {
  airlineCode: string;
  windowMinutes: number;
  transactionCount: number;
  totalVolume: number;
  currencyCode: string;
  errorCount: number;
  errorRate: number;
  latencyP95Ms: number;
  latencyP99Ms: number;
  timestamp: string;
}

export interface AlertEvent {
  alertId: string;
  code: string;
  airlineCode: string;
  ruleName: string;
  windowMinutes: number;
  threshold: number;
  actualValue: number;
  status: string;
  firedAt: string;
}

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private connection: signalR.HubConnection | null = null;

  readonly connected = signal(false);
  readonly transactions = signal<TransactionEvent[]>([]);
  readonly metrics = signal<MetricsEvent[]>([]);
  readonly alerts = signal<AlertEvent[]>([]);

  start(token: string) {
    if (this.connection) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('TransactionCreated', (evt: TransactionEvent) => {
      this.transactions.update((list) => [evt, ...list].slice(0, 50));
    });

    this.connection.on('MetricsUpdated', (evt: MetricsEvent) => {
      this.metrics.update((list) => {
        const idx = list.findIndex(
          (m) => m.airlineCode === evt.airlineCode && m.windowMinutes === evt.windowMinutes
        );
        if (idx >= 0) {
          const updated = [...list];
          updated[idx] = evt;
          return updated;
        }
        return [...list, evt];
      });
    });

    this.connection.on('AlertRaised', (evt: AlertEvent) => {
      this.alerts.update((list) => [evt, ...list].slice(0, 50));
    });

    this.connection.onclose(() => this.connected.set(false));
    this.connection.onreconnected(() => this.connected.set(true));

    this.connection
      .start()
      .then(() => this.connected.set(true))
      .catch((err) => console.error('SignalR connection failed:', err));
  }

  stop() {
    this.connection?.stop();
    this.connection = null;
    this.connected.set(false);
  }
}
