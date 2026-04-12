import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Component } from '@angular/core';
import { AuthService } from './auth.service';

@Component({ template: '' })
class DummyComponent {}

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: 'login', component: DummyComponent }]),
      ],
    });
    service = TestBed.inject(AuthService);
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should not be authenticated initially', () => {
    expect(service.isAuthenticated()).toBe(false);
  });

  it('should have null token initially', () => {
    expect(service.token()).toBeNull();
  });

  it('should have null username initially', () => {
    expect(service.username()).toBeNull();
  });

  it('should read token from localStorage on creation', () => {
    localStorage.setItem('token', 'test-token');
    localStorage.setItem('username', 'testuser');

    // Re-create the service to pick up localStorage
    const newService = new AuthService(
      TestBed.inject(AuthService)['http'],
      TestBed.inject(AuthService)['router']
    );

    expect(newService.token()).toBe('test-token');
    expect(newService.username()).toBe('testuser');
    expect(newService.isAuthenticated()).toBe(true);
  });

  it('should clear localStorage on logout', () => {
    localStorage.setItem('token', 'test-token');
    localStorage.setItem('username', 'testuser');
    service.logout();

    expect(localStorage.getItem('token')).toBeNull();
    expect(localStorage.getItem('username')).toBeNull();
  });
});
