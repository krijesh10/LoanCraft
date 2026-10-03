import { Routes } from '@angular/router';
import { Configurator } from './features/configurator/configurator';
import { Login } from './core/dynamic-forms/AuthService/login/login';
import { authGuard } from './core/dynamic-forms/AuthService/auth.guard';

export const routes: Routes = [
    { path: 'login', component: Login },
    { path: 'configure', component: Configurator, canActivate: [authGuard] },
    { path: '', redirectTo: 'configure', pathMatch: 'full' },
    { path: '**', redirectTo: 'configure' }
];
