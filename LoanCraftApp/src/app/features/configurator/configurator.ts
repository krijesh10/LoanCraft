import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { DynamicForm } from '../../core/dynamic-forms/components/dynamic-form/dynamic-form';
import {
  DynamicControlBase,
  TextboxControl,
  NumberControl,
  SwitchControl,
} from '../../core/dynamic-forms/models/dynamic-control-base';
import { LoanApiService, CreateLoanRequest, LoanResponse } from '../../core/dynamic-forms/services/loan-api.service';
import { WorkflowHubService, StepProgressEvent } from '../../core/dynamic-forms/services/workflow-hub.services';
import { AuthService } from '../../core/dynamic-forms/AuthService/auth.service';

export interface CanvasItem {
  id: string;
  type: 'text' | 'currency' | 'number' | 'switch' | 'command_rest' | 'command_rule';
  title: string;
  key: string;
  value: any;
  config: {
    label: string;
    placeholder?: string;
    required?: boolean;
    min?: number;
    max?: number;
    endpointUrl?: string;
    ruleExpression?: string;
  };
}

@Component({
  selector: 'app-configurator',
  standalone: true,
  imports: [CommonModule, FormsModule, DynamicForm],
  templateUrl: './configurator.html',
  styleUrl: './configurator.css',
})
export class Configurator implements OnInit, OnDestroy {
  private readonly apiService = inject(LoanApiService);
  public readonly hubService = inject(WorkflowHubService);
  public readonly authService = inject(AuthService);

  // Canvas elements state
  canvasItems = signal<CanvasItem[]>([]);
  activeItemId = signal<string | null>(null);

  // Inspector dynamic controls metadata passed to <app-dynamic-form>
  inspectorControls = signal<DynamicControlBase<any>[]>([]);

  // Execution & Telemetry State
  isExecuting = signal<boolean>(false);
  isTelemetryOpen = signal<boolean>(false);
  activeLoan = signal<LoanResponse | null>(null);
  executionError = signal<string | null>(null);

  // Expose SignalR Hub signals
  hubStatus = this.hubService.connectionStatus;
  currentStep = this.hubService.currentStep;
  stepHistory = this.hubService.stepHistory;
  progressPercentage = this.hubService.progressPercentage;

  getStepStatus(stepKey: string): string {
    const step = this.stepHistory().find((s) => s.stepKey === stepKey);
    return step ? step.status : 'Pending';
  }

  async ngOnInit(): Promise<void> {
    this.loadDefaultCanvasItems();
    // Connect to backend SignalR Hub on page open
    await this.hubService.startConnection();
  }

  async ngOnDestroy(): Promise<void> {
    await this.hubService.stopConnection();
  }

  get activeItem(): CanvasItem | undefined {
    return this.canvasItems().find((i) => i.id === this.activeItemId());
  }

  // 1. SELECT AN ELEMENT ON THE CANVAS -> Synthesize metadata schema for Dynamic Inspector
  selectItem(item: CanvasItem): void {
    this.activeItemId.set(item.id);
    this.buildDynamicInspectorSchema(item);
  }

  // Resume Point 1 Core: Dynamically generating metadata without 20 distinct panels
  private buildDynamicInspectorSchema(item: CanvasItem): void {
    const controls: DynamicControlBase<any>[] = [];

    // General Properties (Common to all items)
    controls.push(
      new TextboxControl({
        key: 'label',
        label: 'Field / Node Label',
        value: item.config.label,
        required: true,
        order: 1,
      })
    );

    controls.push(
      new TextboxControl({
        key: 'key',
        label: 'Data Binding Property Key',
        value: item.key,
        required: true,
        order: 2,
      })
    );

    // Type-specific metadata fields
    if (item.type === 'text' || item.type === 'currency' || item.type === 'number') {
      controls.push(
        new TextboxControl({
          key: 'placeholder',
          label: 'Placeholder Text',
          value: item.config.placeholder || '',
          order: 3,
        })
      );

      controls.push(
        new SwitchControl({
          key: 'required',
          label: 'Mandatory Validation',
          value: !!item.config.required,
          placeholder: 'Required in Form',
          order: 4,
        })
      );
    }

    if (item.type === 'currency' || item.type === 'number') {
      controls.push(
        new NumberControl({
          key: 'min',
          label: 'Minimum Allowed Value',
          value: item.config.min ?? 0,
          order: 5,
        })
      );

      controls.push(
        new NumberControl({
          key: 'max',
          label: 'Maximum Allowed Limit',
          value: item.config.max ?? 1000000,
          order: 6,
        })
      );
    }

    if (item.type === 'command_rest') {
      controls.push(
        new TextboxControl({
          key: 'endpointUrl',
          label: 'Target REST Endpoint',
          value: item.config.endpointUrl || 'https://api.mock.credit/score',
          required: true,
          order: 7,
        })
      );
    }

    if (item.type === 'command_rule') {
      controls.push(
        new TextboxControl({
          key: 'ruleExpression',
          label: 'Policy Rule Expression',
          value: item.config.ruleExpression || 'CreditScore >= 680 && DTI <= 45%',
          required: true,
          order: 8,
        })
      );
    }

    this.inspectorControls.set(controls);
  }

  // Form Inspector Emits Changes -> Canvas Updates in Real-Time
  onInspectorChange(event: { values: any; valid: boolean }): void {
    const currentId = this.activeItemId();
    if (!currentId) return;

    this.canvasItems.update((items) =>
      items.map((it) => {
        if (it.id === currentId) {
          return {
            ...it,
            title: event.values.label || it.title,
            key: event.values.key || it.key,
            config: {
              ...it.config,
              label: event.values.label,
              placeholder: event.values.placeholder,
              required: event.values.required,
              min: event.values.min,
              max: event.values.max,
              endpointUrl: event.values.endpointUrl,
              ruleExpression: event.values.ruleExpression,
            },
          };
        }
        return it;
      })
    );
  }

  // 2. PALETTE -> ADD NEW ITEM TO CANVAS
  addItem(type: CanvasItem['type']): void {
    const newId = 'node_' + Math.random().toString(36).substring(2, 7);
    const newItem: CanvasItem = {
      id: newId,
      type,
      title: type === 'command_rest' ? 'Credit Bureau REST API' : `New ${type} Field`,
      key: `prop_${newId}`,
      value: type === 'currency' || type === 'number' ? 0 : '',
      config: {
        label: `Configured ${type}`,
        required: true,
        placeholder: 'Enter details...',
      },
    };

    this.canvasItems.update((items) => [...items, newItem]);
    this.selectItem(newItem);
  }

  removeItem(id: string, event: Event): void {
    event.stopPropagation();
    this.canvasItems.update((items) => items.filter((i) => i.id !== id));
    if (this.activeItemId() === id) {
      this.activeItemId.set(null);
      this.inspectorControls.set([]);
    }
  }

  toggleTelemetry(): void {
    this.isTelemetryOpen.update((open) => !open);
  }

  // 3. EXECUTE BACKEND WORKFLOW ENGINE VIA COMMAND PATTERN + SIGNALR
  async executeWorkflow(): Promise<void> {
    if (this.isExecuting()) return;
    this.isExecuting.set(true);
    this.executionError.set(null);
    this.activeLoan.set(null);
    this.isTelemetryOpen.set(true);
    this.hubService.reset();

    // Ensure SignalR connection is active
    await this.hubService.startConnection();

    // 1. Gather configured data from canvas items
    const items = this.canvasItems();
    const getVal = (key: string, defaultVal: any) => {
      const it = items.find((i) => i.key === key);
      return it !== undefined && it.value !== undefined && it.value !== '' ? it.value : defaultVal;
    };

    const payload: CreateLoanRequest = {
      applicantName: String(getVal('applicantName', 'Antony Krijesh')),
      applicantEmail: String(getVal('applicantEmail', 'krijesh10@gmail.com')),
      requestedAmount: Number(getVal('requestedAmount', 35000)),
      tenureMonths: 36,
      monthlyIncome: Number(getVal('monthlyIncome', 7500)),
      existingDebt: 1200,
      creditScore: Number(getVal('creditScore', 730)),
    };

    try {
      // 2. Submit loan to backend
      console.log('🚀 Submitting loan to backend API:', payload);
      const res = await firstValueFrom(this.apiService.createLoan(payload));
      const loanId = res.loanId || res.LoanId;
      if (!loanId) throw new Error('Backend did not return a valid loanId.');

      console.log('✅ Created loan application ID:', loanId);

      // 3. Join SignalR room for real-time telemetry
      await this.hubService.joinLoanGroup(loanId);

      // 4. Trigger GoF Command Pattern Underwriting Pipeline in .NET
      console.log('⚙️ Executing Underwriting Command Pipeline...');
      await firstValueFrom(this.apiService.runUnderwriting(loanId));

      // 5. Fetch final mutated entity
      const finalLoan = await firstValueFrom(this.apiService.getLoanById(loanId));
      this.activeLoan.set(finalLoan);
      console.log('🎉 Final Underwritten Entity:', finalLoan);
    } catch (err: any) {
      console.error('❌ Workflow execution error:', err);
      this.executionError.set(err.message || 'Failed to execute workflow pipeline.');
    } finally {
      this.isExecuting.set(false);
    }
  }

  private loadDefaultCanvasItems(): void {
    const defaults: CanvasItem[] = [
      {
        id: 'c1',
        type: 'text',
        title: 'Applicant Full Name',
        key: 'applicantName',
        value: 'Antony Krijesh',
        config: { label: 'Applicant Name', required: true, placeholder: 'Enter legal full name' },
      },
      {
        id: 'c2',
        type: 'text',
        title: 'Applicant Email',
        key: 'applicantEmail',
        value: 'krijesh10@gmail.com',
        config: { label: 'Corporate Email', required: true, placeholder: 'name@company.com' },
      },
      {
        id: 'c3',
        type: 'currency',
        title: 'Requested Loan Amount ($)',
        key: 'requestedAmount',
        value: 35000,
        config: { label: 'Loan Amount ($)', required: true, min: 1000, max: 200000 },
      },
      {
        id: 'c4',
        type: 'currency',
        title: 'Monthly Verified Income ($)',
        key: 'monthlyIncome',
        value: 7500,
        config: { label: 'Monthly Income ($)', required: true, min: 500, max: 100000 },
      },
      {
        id: 'c5',
        type: 'number',
        title: 'Credit Score (FICO / CIBIL)',
        key: 'creditScore',
        value: 730,
        config: { label: 'Credit Score', required: true, min: 300, max: 850 },
      },
    ];

    this.canvasItems.set(defaults);
    this.selectItem(defaults[0]);
  }
}
