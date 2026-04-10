import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-900">
      <div class="bg-gray-800 p-8 rounded-lg shadow-xl w-full max-w-md">
        <div class="text-center mb-8">
          <h1 class="text-3xl font-bold text-white">
            ✈ Airline Transaction Monitor
          </h1>
          <p class="text-gray-400 mt-2">Sign in to access the dashboard</p>
        </div>

        <form (ngSubmit)="onSubmit()" class="space-y-4">
          <div>
            <label for="username" class="block text-sm font-medium text-gray-300"
              >Username</label
            >
            <input
              id="username"
              type="text"
              [(ngModel)]="username"
              name="username"
              class="mt-1 block w-full rounded-md bg-gray-700 border-gray-600 text-white px-3 py-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="admin"
              required
            />
          </div>

          <div>
            <label for="password" class="block text-sm font-medium text-gray-300"
              >Password</label
            >
            <input
              id="password"
              type="password"
              [(ngModel)]="password"
              name="password"
              class="mt-1 block w-full rounded-md bg-gray-700 border-gray-600 text-white px-3 py-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="admin"
              required
            />
          </div>

          <button
            type="submit"
            class="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition-colors cursor-pointer"
          >
            Sign In
          </button>

          <p class="text-xs text-gray-500 text-center mt-4">
            Default credentials: admin / admin
          </p>
        </form>
      </div>
    </div>
  `,
})
export class LoginComponent {
  username = '';
  password = '';

  constructor(private auth: AuthService) {}

  onSubmit() {
    if (this.username && this.password) {
      this.auth.login(this.username, this.password);
    }
  }
}
