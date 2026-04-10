import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="min-h-screen bg-gray-900 text-white">
      <!-- Top nav -->
      <nav class="bg-gray-800 border-b border-gray-700">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex items-center justify-between h-16">
            <div class="flex items-center space-x-8">
              <span class="text-xl font-bold">Airline Transaction Monitor</span>
              <div class="flex space-x-4">
                <a
                  routerLink="/dashboard"
                  routerLinkActive="bg-gray-900 text-white"
                  class="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                  >Dashboard</a
                >
                <a
                  routerLink="/transactions"
                  routerLinkActive="bg-gray-900 text-white"
                  class="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                  >Transactions</a
                >
                <a
                  routerLink="/alerts"
                  routerLinkActive="bg-gray-900 text-white"
                  class="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                  >Alerts</a
                >
              </div>
            </div>
            <div class="flex items-center space-x-4">
              <span class="text-sm text-gray-400">{{ auth.username() }}</span>
              <button
                (click)="auth.logout()"
                class="text-gray-400 hover:text-white text-sm cursor-pointer"
              >
                Sign Out
              </button>
            </div>
          </div>
        </div>
      </nav>

      <!-- Page content -->
      <main class="max-w-7xl mx-auto">
        <router-outlet />
      </main>
    </div>
  `,
})
export class LayoutComponent {
  constructor(public auth: AuthService) {}
}
