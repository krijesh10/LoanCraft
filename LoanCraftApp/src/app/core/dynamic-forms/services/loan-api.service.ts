import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface CreateLoanRequest {
    applicantName: string;
    applicantEmail: string;
    requestedAmount: number;
    tenureMonths: number;
    monthlyIncome: number;
    existingDebt: number;
    creditScore: number;
}

export interface LoanResponse {
    id: string;
    applicationNumber?: string;
    applicantName: string;
    applicantEmail?: string;
    requestedAmount: number;
    tenureMonths?: number;
    monthlyIncome: number;
    existingDebt?: number;
    creditScore: number;
    status: 'Draft' | 'Submitted' | 'Underwriting' | 'Approved' | 'Rejected';
    underwriterNotes?: string;
    createdAtUtc?: string;
}

@Injectable({
    providedIn: 'root'
})
export class LoanApiService {
    private readonly http = inject(HttpClient);
    // Backend API URL - Port 7199 HTTPS (matches launchSettings.json)
    private readonly baseUrl = `${environment.apiUrl}/api/loans`;

    /**
     * Fetches all loan applications
     */
    public getAllLoans(): Observable<LoanResponse[]> {
        return this.http.get<LoanResponse[]>(this.baseUrl);
    }

    /**
     * Fetches a single loan application by ID
     */
    public getLoanById(id: string): Observable<LoanResponse> {
        return this.http.get<LoanResponse>(`${this.baseUrl}/${id}`);
    }

    /**
     * Creates a new loan application from dynamic form data
     */
    public createLoan(payload: CreateLoanRequest): Observable<{ loanId?: string; LoanId?: string }> {
        return this.http.post<{ loanId?: string; LoanId?: string }>(this.baseUrl, payload);
    }

    /**
     * Triggers the GoF Command Pattern Underwriting Pipeline in .NET
     */
    public runUnderwriting(id: string): Observable<{ success: boolean; message: string }> {
        return this.http.post<{ success: boolean; message: string }>(`${this.baseUrl}/${id}/execute-underwriting`, {});
    }
}
