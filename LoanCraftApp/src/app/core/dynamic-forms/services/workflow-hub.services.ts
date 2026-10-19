import { Injectable, signal, computed, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from '../AuthService/auth.service';
import { environment } from '../../../../environments/environment';

export interface StepProgressEvent {
    loanId: string;
    stepKey: string;
    stepName: string;
    stepNumber: number;
    totalSteps: number;
    status: 'Pending' | 'Running' | 'Completed' | 'Failed';
    message?: string;
    timestamp?: string;
}

export interface WorkflowCompletedEvent {
    loanId: string;
    status: string;
    summary: string;
    timestamp?: string;
}

@Injectable({
    providedIn: 'root'
})
export class WorkflowHubService {
    // Hub URL - Port 7199 HTTPS (matches backend Program.cs: app.MapHub<WorkflowHub>("/hubs/workflow"))
    private readonly hubUrl = `${environment.apiUrl}/hubs/workflow`;

    private hubConnection?: signalR.HubConnection;
    private readonly authService = inject(AuthService);


    // Angular Signals for reactive, glitch-free UI state
    public connectionStatus = signal<'Disconnected' | 'Connecting' | 'Connected'>('Disconnected');
    public currentStep = signal<StepProgressEvent | null>(null);
    public stepHistory = signal<StepProgressEvent[]>([]);
    public workflowCompleted = signal<WorkflowCompletedEvent | null>(null);

    // Computed signal: Calculates overall progress percentage (e.g., 2 of 4 = 50%)
    public progressPercentage = computed(() => {
        const history = this.stepHistory();
        if (history.length === 0) return 0;
        const completedCount = history.filter(s => s.status === 'Completed').length;
        const runningCount = history.filter(s => s.status === 'Running').length;
        const total = history[0]?.totalSteps || 4;
        const progress = ((completedCount + (runningCount * 0.5)) / total) * 100;
        return Math.min(100, Math.round(progress));
    });

    // Computed signal: True if any step is actively running
    public isRunning = computed(() => {
        const step = this.currentStep();
        return step?.status === 'Running' || this.stepHistory().some(s => s.status === 'Running');
    });

    /**
     * Initializes and starts the SignalR WebSocket connection
     */
    public async startConnection(): Promise<void> {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        this.connectionStatus.set('Connecting');

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl, {
                accessTokenFactory: () => this.authService.getToken() ?? '',
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
            })
            .withAutomaticReconnect()
            .build();

        // 1. Listen for StepStarted
        this.hubConnection.on('StepStarted', (data: any) => {
            console.log('📡 SignalR StepStarted:', data);
            const event: StepProgressEvent = {
                loanId: data.loanId,
                stepKey: data.stepKey,
                stepName: data.stepName,
                stepNumber: data.stepIndex,
                totalSteps: data.totalSteps,
                status: 'Running',
                message: `Executing ${data.stepName}...`,
                timestamp: data.timestamp
            };
            this.handleStepProgress(event);
        });

        // 2. Listen for StepCompleted
        this.hubConnection.on('StepCompleted', (data: any) => {
            console.log('✅ SignalR StepCompleted:', data);
            const currentHistory = this.stepHistory();
            const existing = currentHistory.find(s => s.stepKey === data.stepKey);
            const event: StepProgressEvent = {
                loanId: data.loanId,
                stepKey: data.stepKey,
                stepName: existing?.stepName || data.stepKey,
                stepNumber: existing?.stepNumber || currentHistory.length || 1,
                totalSteps: existing?.totalSteps || 4,
                status: 'Completed',
                message: `Completed ${existing?.stepName || data.stepKey}`,
                timestamp: data.timestamp
            };
            this.handleStepProgress(event);
        });

        // 3. Listen for StepFailed
        this.hubConnection.on('StepFailed', (data: any) => {
            console.error('❌ SignalR StepFailed:', data);
            const currentHistory = this.stepHistory();
            const existing = currentHistory.find(s => s.stepKey === data.stepKey);
            const event: StepProgressEvent = {
                loanId: data.loanId,
                stepKey: data.stepKey,
                stepName: existing?.stepName || data.stepKey,
                stepNumber: existing?.stepNumber || 1,
                totalSteps: existing?.totalSteps || 4,
                status: 'Failed',
                message: data.error,
                timestamp: data.timestamp
            };
            this.handleStepProgress(event);
        });

        // 4. Listen for WorkflowCompleted
        this.hubConnection.on('WorkflowCompleted', (data: any) => {
            console.log('🎉 SignalR WorkflowCompleted:', data);
            this.workflowCompleted.set({
                loanId: data.loanId,
                status: data.status,
                summary: data.summary,
                timestamp: data.timestamp
            });
        });

        this.hubConnection.onreconnecting(() => this.connectionStatus.set('Connecting'));
        this.hubConnection.onreconnected(() => this.connectionStatus.set('Connected'));
        this.hubConnection.onclose(() => this.connectionStatus.set('Disconnected'));

        try {
            await this.hubConnection.start();
            this.connectionStatus.set('Connected');
            console.log('✅ SignalR WorkflowHub Connected');
        } catch (err) {
            this.connectionStatus.set('Disconnected');
            console.error('❌ Error connecting to SignalR WorkflowHub:', err);
        }
    }

    /**
     * Joins a specific loan room so we only receive events for this loan
     */
    public async joinLoanGroup(loanId: string): Promise<void> {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('JoinLoanExecutionGroup', loanId);
            console.log(`Joined SignalR group for loan: ${loanId}`);
        }
    }

    /**
     * Resets the progress tracker for a new run
     */
    public reset(): void {
        this.currentStep.set(null);
        this.stepHistory.set([]);
        this.workflowCompleted.set(null);
    }

    /**
     * Stops the connection
     */
    public async stopConnection(): Promise<void> {
        if (this.hubConnection) {
            await this.hubConnection.stop();
            this.connectionStatus.set('Disconnected');
        }
    }

    private handleStepProgress(data: StepProgressEvent): void {
        this.currentStep.set(data);

        this.stepHistory.update(history => {
            const index = history.findIndex(s => s.stepKey === data.stepKey);
            if (index >= 0) {
                const updated = [...history];
                updated[index] = data;
                return updated;
            }
            return [...history, data];
        });
    }
}
