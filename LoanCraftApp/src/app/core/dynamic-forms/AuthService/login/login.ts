import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { KeycloakService } from '../../services/keycloak.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './login.html',
    styleUrl: './login.css'
})
export class Login {
    private readonly auth = inject(AuthService);
    private readonly router = inject(Router);
    private readonly keycloakService = inject(KeycloakService);

    email = signal('underwriter@loancraft.com');
    password = signal('Password123!');
    isLoading = signal(false);
    errorMessage = signal<string | null>(null);

    onSubmit(): void {
        this.isLoading.set(true);
        this.errorMessage.set(null);

        this.auth.login({ email: this.email(), password: this.password() }).subscribe({
            next: () => {
                this.isLoading.set(false);
                this.router.navigate(['/configure']); // Go to Configurator Studio
            },
            error: (err) => {
                this.isLoading.set(false);
                this.errorMessage.set(err.error?.message || 'Invalid email or password.');
            }
        });
    }

    loginWithKeycloak(): void {
        this.keycloakService.login();
    }


    fillDemoCredentials(): void {
        this.email.set('underwriter@loancraft.com');
        this.password.set('Password123!');
    }
}
