import { ErrorHandler, Injectable, inject, NgZone } from '@angular/core';
import { ToastService } from '../services/toast.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
    private readonly toast = inject(ToastService);
    private readonly zone = inject(NgZone);

    handleError(error: any): void {
        console.error('[LoanCraft Global Client Error]:', error);

        // Run inside Angular NgZone so signals immediately trigger UI updates
        this.zone.run(() => {
            const msg = error?.message || 'An unexpected client interface crash occurred.';
            this.toast.showWarning('Client Runtime Exception', msg);
        });
    }
}
