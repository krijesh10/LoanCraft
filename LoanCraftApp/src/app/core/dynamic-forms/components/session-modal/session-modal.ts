import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SessionService } from '../../services/session.service';

@Component({
  selector: 'app-session-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './session-modal.html',
  styleUrl: './session-modal.css'
})
export class SessionModal {
  public readonly sessionService = inject(SessionService);
}
