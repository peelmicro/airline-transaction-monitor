import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

interface LoginResponse {
  token: string;
  username: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenSignal = signal<string | null>(localStorage.getItem('token'));
  private usernameSignal = signal<string | null>(localStorage.getItem('username'));

  readonly token = this.tokenSignal.asReadonly();
  readonly username = this.usernameSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.tokenSignal());

  constructor(private http: HttpClient, private router: Router) {}

  login(username: string, password: string) {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, { username, password })
      .subscribe({
        next: (response) => {
          this.tokenSignal.set(response.token);
          this.usernameSignal.set(response.username);
          localStorage.setItem('token', response.token);
          localStorage.setItem('username', response.username);
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          console.error('Login failed:', err);
        },
      });
  }

  logout() {
    this.tokenSignal.set(null);
    this.usernameSignal.set(null);
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    this.router.navigate(['/login']);
  }
}
