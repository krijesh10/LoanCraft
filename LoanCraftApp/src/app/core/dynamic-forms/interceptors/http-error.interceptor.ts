import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';
import { ProblemDetails } from '../models/problem-details';
import { AuthService } from '../AuthService/auth.service';

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
    const toast = inject(ToastService);
    const auth = inject(AuthService);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            // 1. Network / Server Unreachable
            if (error.status === 0) {
                toast.showError('Server Unreachable', 'Unable to connect to LoanCraft API. Check connection or backend status.');
                return throwError(() => error);
            }

            // 2. Parse RFC 7807 ProblemDetails from Backend
            const problem = (error.error as ProblemDetails) || {};
            const traceId = problem.traceId || error.headers?.get('X-Correlation-ID') || undefined;
            const title = problem.title || error.statusText || 'Request Error';
            const detail = problem.detail || error.message || 'An unexpected error occurred.';

            switch (error.status) {
                case 400: // Bad Request / Validation Failure
                    if (problem.errors && Object.keys(problem.errors).length > 0) {
                        const firstErr = Object.values(problem.errors)[0]?.[0];
                        toast.showError('Validation Failed', firstErr || detail, traceId);
                    } else {
                        toast.showError('Validation Error', detail, traceId);
                    }
                    break;

                case 401: // Unauthorized (Token Expired / Missing)
                    // Don't toast for normal failed login credentials; let the login form show it
                    if (!req.url.includes('/api/auth/login')) {
                        toast.showWarning('Session Invalid', 'Your security token has expired. Please sign in again.');
                        auth.logout();
                    }
                    break;

                case 403: // Forbidden
                    toast.showError('Access Denied', 'You lack sufficient permissions for this loan operation.', traceId);
                    break;

                case 404: // Not Found
                    toast.showError('Resource Not Found', detail, traceId);
                    break;

                case 409: // Conflict
                    toast.showWarning('Conflict', detail);
                    break;

                case 500: // Server Error
                default:
                    toast.showError('Internal Server Error', detail, traceId);
                    break;
            }

            return throwError(() => error);
        })
    );
};
