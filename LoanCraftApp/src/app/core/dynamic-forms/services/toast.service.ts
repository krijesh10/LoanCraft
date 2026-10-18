import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
    id: string;
    type: 'error' | 'warning' | 'success' | 'info';
    title: string;
    message: string;
    traceId?: string;
    durationMs?: number;
}

@Injectable({
    providedIn: 'root'
})
export class ToastService {
    public toasts = signal<ToastMessage[]>([]);

    showError(title: string, message: string, traceId?: string): void {
        this.addToast({
            id: crypto.randomUUID(),
            type: 'error',
            title,
            message,
            traceId,
            durationMs: 8000 // 8 seconds for errors so users can copy trace ID
        });
    }

    showWarning(title: string, message: string): void {
        this.addToast({
            id: crypto.randomUUID(),
            type: 'warning',
            title,
            message,
            durationMs: 5000
        });
    }

    showSuccess(title: string, message: string): void {
        this.addToast({
            id: crypto.randomUUID(),
            type: 'success',
            title,
            message,
            durationMs: 4000
        });
    }

    remove(id: string): void {
        this.toasts.update(list => list.filter(t => t.id !== id));
    }

    private addToast(toast: ToastMessage): void {
        this.toasts.update(list => [...list, toast]);

        if (toast.durationMs && toast.durationMs > 0) {
            setTimeout(() => this.remove(toast.id), toast.durationMs);
        }
    }
}
