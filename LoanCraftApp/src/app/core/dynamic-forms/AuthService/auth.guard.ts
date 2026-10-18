import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { KeycloakService } from '../services/keycloak.service';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
    const keycloak = inject(KeycloakService);
    const localAuth = inject(AuthService);
    const router = inject(Router);

    // User is authenticated if logged in through Keycloak SSO OR local credentials
    if (keycloak.isAuthenticated() || localAuth.isAuthenticated()) {
        return true;
    }

    router.navigate(['/login']);
    return false;
};
