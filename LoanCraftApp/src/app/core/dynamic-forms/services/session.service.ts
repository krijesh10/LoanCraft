import { Injectable, signal, computed, inject, effect } from '@angular/core';
import { ToastService } from './toast.service';
import { AuthService } from '../AuthService/auth.service';

@Injectable({
    providedIn: 'root'
})
export class SessionService {
    private readonly auth = inject(AuthService);
    private readonly toast = inject(ToastService);

    // Warning modal visibility and remaining countdown in seconds
    public isWarningOpen = signal<boolean>(false);
    public remainingSeconds = signal<number>(120);
    public formattedCountdown = computed(() => {
        const s = this.remainingSeconds();
        const mins = Math.floor(s / 60).toString().padStart(2, '0');
        const secs = (s % 60).toString().padStart(2, '0');
        return `${mins}:${secs}`;
    });

    private countdownTimer: any = null;
    private checkInterval: any = null;

    constructor() {
        // Whenever authentication state changes, manage the check loop
        effect(() => {
            if (this.auth.isAuthenticated()) {
                this.startSessionMonitoring();
            } else {
                this.stopSessionMonitoring();
            }
        });
    }

    private startSessionMonitoring(): void {
        this.stopSessionMonitoring();
        // Check every 15 seconds if session is close to expiry
        this.checkInterval = setInterval(() => this.checkExpiration(), 15000);
        this.checkExpiration();
    }

    private checkExpiration(): void {
        const session = this.auth.currentUser();
        if (!session?.expiresAtUtc) return;

        const expiresAt = new Date(session.expiresAtUtc).getTime();
        const now = Date.now();
        const diffSec = Math.floor((expiresAt - now) / 1000);

        // If less than 2 minutes (120s) remain and modal is not already open:
        if (diffSec <= 120 && diffSec > 0 && !this.isWarningOpen()) {
            this.promptWarning(diffSec);
        } else if (diffSec <= 0) {
            this.handleSessionExpired();
        }
    }

    private promptWarning(secondsLeft: number): void {
        this.remainingSeconds.set(secondsLeft);
        this.isWarningOpen.set(true);

        clearInterval(this.countdownTimer);
        this.countdownTimer = setInterval(() => {
            this.remainingSeconds.update(s => {
                if (s <= 1) {
                    clearInterval(this.countdownTimer);
                    this.handleSessionExpired();
                    return 0;
                }
                return s - 1;
            });
        }, 1000);
    }

    public extendSession(): void {
        this.auth.extendSession().subscribe({
            next: () => {
                this.isWarningOpen.set(false);
                clearInterval(this.countdownTimer);
                this.toast.showSuccess('Session Extended', 'Your underwriting session has been extended by 2 hours.');
            },
            error: () => {
                this.handleSessionExpired();
            }
        });
    }

    public logoutNow(): void {
        this.isWarningOpen.set(false);
        clearInterval(this.countdownTimer);
        this.auth.logout();
    }

    public simulateExpiryWarning(): void {
        // Development helper: Instantly triggers a 30s warning for testing
        this.promptWarning(30);
    }

    private handleSessionExpired(): void {
        this.isWarningOpen.set(false);
        clearInterval(this.countdownTimer);
        this.auth.logout();
        this.toast.showWarning('Session Expired', 'You were automatically logged out due to inactivity to protect sensitive financial data.');
    }

    private stopSessionMonitoring(): void {
        clearInterval(this.checkInterval);
        clearInterval(this.countdownTimer);
        this.isWarningOpen.set(false);
    }
}
