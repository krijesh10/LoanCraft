import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface UserSession {
    token: string;
    fullName: string;
    email: string;
    role: string;
    expiresAtUtc: string;
}

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly http = inject(HttpClient);
    private readonly router = inject(Router);
    private readonly baseUrl = `${environment.apiUrl}/api/auth`;

    // Angular Signals for reactive authentication state
    public currentUser = signal<UserSession | null>(this.getStoredSession());
    public isAuthenticated = computed(() => !!this.currentUser()?.token);

    public login(credentials: { email: string; password: string }): Observable<UserSession> {
        return this.http.post<UserSession>(`${this.baseUrl}/login`, credentials).pipe(
            tap(session => {
                localStorage.setItem('loancraft_session', JSON.stringify(session));
                this.currentUser.set(session);
            })
        );
    }

    public logout(): void {
        localStorage.removeItem('loancraft_session');
        this.currentUser.set(null);
        this.router.navigate(['/login']);
    }

    public getToken(): string | null {
        return this.currentUser()?.token ?? null;
    }

    private getStoredSession(): UserSession | null {
        const raw = localStorage.getItem('loancraft_session');
        return raw ? JSON.parse(raw) : null;
    }

    public extendSession(): Observable<UserSession> {
        return this.http.post<UserSession>(`${this.baseUrl}/refresh`, {}).pipe(
            tap(session => {
                localStorage.setItem('loancraft_session', JSON.stringify(session));
                this.currentUser.set(session);
            })
        );
    }

}
