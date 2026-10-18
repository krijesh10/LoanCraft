import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastMessage } from '../../services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast-container.html',
  styleUrl: './toast-container.css'
})
export class ToastContainer {
  public readonly toastService = inject(ToastService);

  copyTraceId(traceId: string, event: MouseEvent): void {
    event.stopPropagation();
    navigator.clipboard.writeText(traceId);
    this.toastService.showSuccess('Copied to Clipboard', `Trace ID: ${traceId}`);
  }
}
