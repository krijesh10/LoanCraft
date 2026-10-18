import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SessionModal } from './core/dynamic-forms/components/session-modal/session-modal';
import { ToastContainer } from './core/dynamic-forms/components/toast-container/toast-container';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastContainer, SessionModal],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('LoanCraft Studio');
}
