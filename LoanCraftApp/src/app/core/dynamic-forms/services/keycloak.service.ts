import { Injectable, signal } from '@angular/core';
import Keycloak from 'keycloak-js';
import { environment } from '../../../../environments/environment';

export interface KeycloakUserProfile {
    username: string;
    email: string;
    fullName: string;
    roles: string[];
}

@Injectable({
    providedIn: 'root'
})
export class KeycloakService {
    private keycloakInstance: Keycloak | null = null;
    public isInitialized = signal<boolean>(false);
    public isAuthenticated = signal<boolean>(false);
    public userProfile = signal<KeycloakUserProfile | null>(null);

    /**
     * Initializes Keycloak OIDC with Authorization Code Flow + PKCE (S256)
     */
    public async init(): Promise<boolean> {
        this.keycloakInstance = new Keycloak({
            url: environment.keycloakUrl,
            realm: 'LoanCraft',
            clientId: 'loancraft-app'
        });

        try {
            const authenticated = await this.keycloakInstance.init({
                onLoad: 'check-sso', // Checks if user is already logged in via SSO session
                pkceMethod: 'S256',  // Modern OAuth 2.0 PKCE standard!
                checkLoginIframe: false
            });

            this.isAuthenticated.set(authenticated);
            this.isInitialized.set(true);

            if (authenticated) {
                this.extractUserProfile();
            }

            return authenticated;
        } catch (error) {
            console.error('[Keycloak] Initialization failed:', error);
            this.isInitialized.set(true);
            return false;
        }
    }

    /**
     * Redirects to Keycloak login screen
     */
    public login(): Promise<void> {
        if (!this.keycloakInstance) throw new Error('Keycloak not initialized');
        return this.keycloakInstance.login({
            redirectUri: window.location.origin + '/configure'
        });
    }

    /**
     * Single Sign-Out (SLO)
     */
    public logout(): Promise<void> {
        if (!this.keycloakInstance) throw new Error('Keycloak not initialized');
        return this.keycloakInstance.logout({
            redirectUri: window.location.origin + '/login'
        });
    }

    /**
     * Returns current JWT Access Token
     */
    public getToken(): string | null {
        return this.keycloakInstance?.token ?? null;
    }

    /**
     * Automatically refreshes token if it's within 30 seconds of expiring
     */
    public async refreshToken(): Promise<string | null> {
        if (!this.keycloakInstance) return null;
        try {
            const refreshed = await this.keycloakInstance.updateToken(30);
            if (refreshed) {
                console.log('[Keycloak] Access Token refreshed successfully via OIDC.');
            }
            return this.keycloakInstance.token ?? null;
        } catch (error) {
            console.error('[Keycloak] Failed to refresh token, session expired:', error);
            this.logout();
            return null;
        }
    }

    private extractUserProfile(): void {
        const tokenParsed: any = this.keycloakInstance?.tokenParsed;
        if (!tokenParsed) return;

        const roles: string[] = tokenParsed.realm_access?.roles ?? [];

        this.userProfile.set({
            username: tokenParsed.preferred_username || '',
            email: tokenParsed.email || '',
            fullName: tokenParsed.name || tokenParsed.preferred_username || 'Underwriter',
            roles: roles
        });
    }
}
