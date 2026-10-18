import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { KeycloakService } from '../services/keycloak.service';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const keycloak = inject(KeycloakService);
    const localAuth = inject(AuthService);

    // Prefer Keycloak OIDC Access Token if logged in via SSO, otherwise fallback to local JWT
    const token = keycloak.getToken() || localAuth.getToken();

    if (token) {
        const authReq = req.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        });
        return next(authReq);
    }

    return next(req);
};
