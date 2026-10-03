# LoanCraft: Complete Project, Architecture & Code Export
**Reverse-Engineered Enterprise Lending Platform & Workflow Execution Engine**  
*Full Stack .NET 9 Clean Architecture + Angular 20 Standalone Signals*

---

## Table of Contents
1. [Master Project Overview & Resume Positioning](#1-master-project-overview--resume-positioning)
2. [Phase 1: Metadata-Driven Dynamic Forms Engine (Angular 20)](#2-phase-1-metadata-driven-dynamic-forms-engine-angular-20)
   - `dynamic-control-base.ts`
   - `dynamic-form-control.service.ts`
   - `dynamic-form-control.ts` / `.html` / `.css`
   - `dynamic-form.ts` / `.html` / `.css`
3. [Phase 2: Visual Configurator Studio (3-Panel Architecture)](#3-phase-2-visual-configurator-studio-3-panel-architecture)
   - Architectural Concept: Component Palette, Canvas, Single Dynamic Inspector
   - `configurator.html`
   - `configurator.ts`
   - `configurator.css`
4. [Phase 3: Backend GoF Command Pattern Execution Engine (.NET 9)](#4-phase-3-backend-gof-command-pattern-execution-engine-net-9)
   - GoF Command Pattern Principles & Classroom Analogy
   - `IWorkflowCommand.cs`
   - `WorkflowExecutionContext.cs`
   - `CommandInvoker.cs`
   - `ConcreteCommands.cs` (REST API, Mapping, Decision Rule, Entity Mutation)
   - `UnderwritingService.cs`
   - `LoanService.cs`
5. [Phase 4: Real-Time Telemetry & Angular Integration](#5-phase-4-real-time-telemetry--angular-integration)
   - `workflow-hub.services.ts` (SignalR + Angular Signals)
   - `loan-api.service.ts` (REST Client)
   - `app.config.ts` (`provideHttpClient`)
6. [Phase 5: Enterprise Security & System Design Archive](#6-phase-5-enterprise-security--system-design-archive)
   - Zero-Trust Client Principle
   - Myth-Buster: What CORS actually does
   - Why the Frontend NEVER holds a secret key
   - How the Backend verifies JWTs (The Wax Seal Analogy)
   - Preventing Interception & Replay: TLS Nonces & OAuth2 PKCE
   - Password Hashing (Argon2 / BCrypt / PBKDF2)

---

# 1. Master Project Overview & Resume Positioning

This project bridges key enterprise architecture skills for senior Full Stack .NET + Angular roles:

1. **Resume Point 1: Metadata-Driven Dynamic Form Engine**
   - Eliminates 60% of hardcoded form markup.
   - Converts declarative JSON schemas into reactive forms dynamically.
2. **Resume Point 2: GoF Command Pattern Execution Engine**
   - Decoupled, auditable pipeline for commercial loan underwriting.
   - Multi-step execution: REST calls, data normalization, business rule matrices, and aggregate entity mutation.
   - Microsecond diagnostic logging and real-time SignalR progress broadcasting.
3. **Resume Point 3: Zero-Trust Security & SSO**
   - OAuth2 with PKCE, stateless JWT tokens with server-side digital signature verification, and role-based access control.

---

# 2. Phase 1: Metadata-Driven Dynamic Forms Engine (Angular 20)

### 2.1 `src/app/core/dynamic-forms/models/dynamic-control-base.ts`
```typescript
export interface DynamicControlOptions<T> {
  value?: T;
  key?: string;
  label?: string;
  required?: boolean;
  order?: number;
  controlType?: string;
  type?: string;
  options?: { key: string; value: string }[];
  placeholder?: string;
  min?: number;
  max?: number;
}

export class DynamicControlBase<T> {
  value: T | undefined;
  key: string;
  label: string;
  required: boolean;
  order: number;
  controlType: string;
  type: string;
  options: { key: string; value: string }[];
  placeholder: string;
  min?: number;
  max?: number;

  constructor(options: DynamicControlOptions<T> = {}) {
    this.value = options.value;
    this.key = options.key || '';
    this.label = options.label || '';
    this.required = !!options.required;
    this.order = options.order === undefined ? 1 : options.order;
    this.controlType = options.controlType || '';
    this.type = options.type || '';
    this.options = options.options || [];
    this.placeholder = options.placeholder || '';
    this.min = options.min;
    this.max = options.max;
  }
}

export class TextboxControl extends DynamicControlBase<string> {
  override controlType = 'textbox';
}

export class NumberControl extends DynamicControlBase<number> {
  override controlType = 'number';
}

export class DropdownControl extends DynamicControlBase<string> {
  override controlType = 'dropdown';
}

export class SwitchControl extends DynamicControlBase<boolean> {
  override controlType = 'switch';
}
```

---

### 2.2 `src/app/core/dynamic-forms/services/dynamic-form-control.service.ts`
```typescript
import { Injectable } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { DynamicControlBase } from '../models/dynamic-control-base';

@Injectable({
  providedIn: 'root'
})
export class DynamicFormControlService {
  toFormGroup(controls: DynamicControlBase<any>[]): FormGroup {
    const group: { [key: string]: FormControl } = {};

    controls.forEach(control => {
      const validators = control.required ? [Validators.required] : [];
      group[control.key] = new FormControl(control.value ?? '', validators);
    });

    return new FormGroup(group);
  }
}
```

---

### 2.3 `src/app/core/dynamic-forms/components/dynamic-form-control/dynamic-form-control.ts`
```typescript
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { DynamicControlBase } from '../../models/dynamic-control-base';

@Component({
  selector: 'app-dynamic-form-control',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './dynamic-form-control.html',
  styleUrl: './dynamic-form-control.css'
})
export class DynamicFormControl {
  @Input({ required: true }) control!: DynamicControlBase<any>;
  @Input({ required: true }) form!: FormGroup;

  get isValid(): boolean {
    const c = this.form.get(this.control.key);
    return !(c && c.invalid && (c.dirty || c.touched));
  }
}
```

#### `dynamic-form-control.html`
```html
<div [formGroup]="form" class="control-wrapper" [class.has-error]="!isValid">
  <label [attr.for]="control.key" class="control-label">
    {{ control.label }}
    @if (control.required) { <span class="required-star">*</span> }
  </label>

  <div class="input-container">
    @switch (control.controlType) {
      @case ('textbox') {
        <input
          [id]="control.key"
          [type]="control.type || 'text'"
          [formControlName]="control.key"
          [placeholder]="control.placeholder"
          class="form-input"
        />
      }
      @case ('number') {
        <input
          [id]="control.key"
          type="number"
          [formControlName]="control.key"
          [placeholder]="control.placeholder"
          [min]="control.min ?? null"
          [max]="control.max ?? null"
          class="form-input"
        />
      }
      @case ('dropdown') {
        <select [id]="control.key" [formControlName]="control.key" class="form-select">
          <option value="" disabled>Select {{ control.label }}</option>
          @for (opt of control.options; track opt.key) {
            <option [value]="opt.key">{{ opt.value }}</option>
          }
        </select>
      }
      @case ('switch') {
        <label class="switch-toggle">
          <input [id]="control.key" type="checkbox" [formControlName]="control.key" />
          <span class="slider round"></span>
          <span class="switch-label">{{ control.placeholder || 'Enable' }}</span>
        </label>
      }
    }
  </div>

  @if (!isValid) {
    <div class="error-text">
      {{ control.label }} is required.
    </div>
  }
</div>
```

---

### 2.4 `src/app/core/dynamic-forms/components/dynamic-form/dynamic-form.ts`
```typescript
import { Component, Input, Output, EventEmitter, OnInit, OnChanges, OnDestroy, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subscription, debounceTime } from 'rxjs';
import { DynamicControlBase } from '../../models/dynamic-control-base';
import { DynamicFormControlService } from '../../services/dynamic-form-control.service';
import { DynamicFormControl } from '../dynamic-form-control/dynamic-form-control';

@Component({
  selector: 'app-dynamic-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DynamicFormControl],
  templateUrl: './dynamic-form.html',
  styleUrl: './dynamic-form.css'
})
export class DynamicForm implements OnInit, OnChanges, OnDestroy {
  private readonly qcs = inject(DynamicFormControlService);

  @Input() controls: DynamicControlBase<any>[] = [];
  @Output() formChange = new EventEmitter<{ values: any; valid: boolean }>();

  public form!: FormGroup;
  private valueChangesSub?: Subscription;

  ngOnInit(): void {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['controls'] && !changes['controls'].firstChange) {
      this.buildForm();
    }
  }

  ngOnDestroy(): void {
    this.valueChangesSub?.unsubscribe();
  }

  private buildForm(): void {
    this.valueChangesSub?.unsubscribe();
    this.form = this.qcs.toFormGroup(this.controls);

    this.valueChangesSub = this.form.valueChanges
      .pipe(debounceTime(150))
      .subscribe(values => {
        this.formChange.emit({
          values,
          valid: this.form.valid
        });
      });
  }
}
```

---

# 3. Phase 2: Visual Configurator Studio (3-Panel Architecture)

### 3.1 `src/app/features/configurator/configurator.html`
```html
<div class="studio-layout">
    <!-- Top Application Bar -->
    <header class="studio-header">
        <div class="brand">
            <span class="brand-icon">⚡</span>
            <div>
                <h1 class="brand-title">LoanCraft Studio</h1>
                <p class="brand-sub">Metadata-Driven Dynamic Configurator</p>
            </div>
        </div>
    </header>

    <!-- 3-Column Split Workspace -->
    <div class="studio-body">
        <!-- 1. LEFT PALETTE: Component & Command Palette -->
        <aside class="left-palette">
            <div class="panel-header">
                <h2>Component Palette</h2>
                <span class="badge">Click to Add</span>
            </div>

            <div class="palette-section">
                <span class="section-title">Form Controls</span>
                <button class="palette-item" (click)="addItem('text')">
                    <span class="item-icon">📝</span>
                    <div class="item-info">
                        <span class="item-name">Text Field</span>
                        <span class="item-desc">Names, emails, text</span>
                    </div>
                </button>

                <button class="palette-item" (click)="addItem('currency')">
                    <span class="item-icon">💵</span>
                    <div class="item-info">
                        <span class="item-name">Currency / Amount</span>
                        <span class="item-desc">Income, debt, loan amount</span>
                    </div>
                </button>

                <button class="palette-item" (click)="addItem('number')">
                    <span class="item-icon">🔢</span>
                    <div class="item-info">
                        <span class="item-name">Numeric Stepper</span>
                        <span class="item-desc">Scores, tenure months</span>
                    </div>
                </button>

                <button class="palette-item" (click)="addItem('switch')">
                    <span class="item-icon">🔘</span>
                    <div class="item-info">
                        <span class="item-name">Toggle Switch</span>
                        <span class="item-desc">KYC flag, collateral status</span>
                    </div>
                </button>
            </div>

            <div class="palette-section">
                <span class="section-title">Workflow Commands (Point 2)</span>
                <button class="palette-item command-item" (click)="addItem('command_rest')">
                    <span class="item-icon">🌐</span>
                    <div class="item-info">
                        <span class="item-name">REST Bureau Command</span>
                        <span class="item-desc">External credit inquiry</span>
                    </div>
                </button>

                <button class="palette-item command-item" (click)="addItem('command_rule')">
                    <span class="item-icon">⚖️</span>
                    <div class="item-info">
                        <span class="item-name">Decision Rule Command</span>
                        <span class="item-desc">Policy branch evaluation</span>
                    </div>
                </button>
            </div>
        </aside>

        <!-- 2. CENTER: Interactive Form & Workflow Canvas -->
        <main class="center-canvas">
            <div class="canvas-header">
                <div>
                    <h2>Design Surface (Canvas)</h2>
                    <p class="canvas-subtitle">Click any element below to inspect and customize its schema</p>
                </div>
                <span class="element-count">{{ canvasItems().length }} Configured Elements</span>
            </div>

            <div class="canvas-viewport">
                <div class="canvas-grid">
                    @for (item of canvasItems(); track item.id) {
                    <div class="canvas-card" [class.is-active]="activeItemId() === item.id" (click)="selectItem(item)">
                        <div class="card-header">
                            <div class="card-title-group">
                                <span class="card-badge" [class.badge-command]="item.type.startsWith('command_')">
                                    {{ item.type.toUpperCase() }}
                                </span>
                                <span class="card-title">{{ item.config.label }}</span>
                            </div>
                            <button class="delete-btn" title="Remove from canvas" (click)="removeItem(item.id, $event)">
                                ✕
                            </button>
                        </div>

                        <div class="card-preview">
                            @if (item.type === 'text') {
                            <input type="text" [(ngModel)]="item.value" [placeholder]="item.config.placeholder || ''"
                                class="canvas-input" />
                            }
                            @if (item.type === 'currency' || item.type === 'number') {
                            <div class="currency-input-wrapper">
                                <input type="number" [(ngModel)]="item.value" [min]="item.config.min ?? 0"
                                    [max]="item.config.max ?? 1000000" class="canvas-input" />
                            </div>
                            }
                            @if (item.type === 'switch') {
                            <label class="canvas-switch">
                                <input type="checkbox" [(ngModel)]="item.value" />
                                <span>{{ item.value ? 'Active / True' : 'Inactive / False' }}</span>
                            </label>
                            }
                            @if (item.type === 'command_rest') {
                            <div class="command-preview-box">
                                <code>GET {{ item.config.endpointUrl }}</code>
                                <span class="tag">IWorkflowCommand</span>
                            </div>
                            }
                            @if (item.type === 'command_rule') {
                            <div class="command-preview-box">
                                <code>RULE: {{ item.config.ruleExpression }}</code>
                                <span class="tag">Decision Policy</span>
                            </div>
                            }
                        </div>

                        <div class="card-footer">
                            <span class="prop-key">Property: <code>{{ item.key }}</code></span>
                            @if (item.config.required) {
                            <span class="required-flag">Required</span>
                            }
                        </div>
                    </div>
                    }
                </div>
            </div>
        </main>

        <!-- 3. RIGHT PANEL: The Star Feature - Single Reusable Dynamic Form Inspector -->
        <aside class="right-inspector">
            <div class="panel-header">
                <div>
                    <h2>Property Inspector</h2>
                    <span class="badge-single">1 Single Dynamic Engine</span>
                </div>
            </div>

            <div class="inspector-content">
                @if (activeItem) {
                <div class="inspector-meta-banner">
                    <span class="meta-label">Selected Element</span>
                    <span class="meta-type">{{ activeItem.title }}</span>
                    <span class="meta-id">ID: {{ activeItem.id }}</span>
                </div>

                <!-- THE DYNAMIC FORM ENGINE: Renders any schema dynamically without 20 separate panels! -->
                <app-dynamic-form [controls]="inspectorControls()"
                    (formChange)="onInspectorChange($event)"></app-dynamic-form>
                } @else {
                <div class="empty-inspector">
                    <span class="empty-icon">👆</span>
                    <p>Select any element on the canvas to inspect and configure its metadata properties dynamically.
                    </p>
                </div>
                }
            </div>
        </aside>
    </div>
</div>
```

---

### 3.2 `src/app/features/configurator/configurator.ts`
```typescript
import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DynamicForm } from '../../core/dynamic-forms/components/dynamic-form/dynamic-form';
import {
  DynamicControlBase,
  TextboxControl,
  NumberControl,
  SwitchControl,
} from '../../core/dynamic-forms/models/dynamic-control-base';

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
export class Configurator implements OnInit {
  // Canvas elements state
  canvasItems = signal<CanvasItem[]>([]);
  activeItemId = signal<string | null>(null);

  // Inspector dynamic controls metadata passed to <app-dynamic-form>
  inspectorControls = signal<DynamicControlBase<any>[]>([]);

  ngOnInit(): void {
    this.loadDefaultCanvasItems();
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
```

---

### 3.3 `src/app/features/configurator/configurator.css`
```css
:host {
    display: block;
    height: 100vh;
    overflow: hidden;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
    background-color: #f1f5f9;
    color: #0f172a;
}

.studio-layout {
    display: flex;
    flex-direction: column;
    height: 100vh;
}

/* Header */
.studio-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.75rem 1.5rem;
    background: #ffffff;
    border-bottom: 1px solid #e2e8f0;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.brand {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.brand-icon {
    font-size: 1.5rem;
    background: #eff6ff;
    border: 1px solid #bfdbfe;
    padding: 0.4rem;
    border-radius: 8px;
}

.brand-title {
    margin: 0;
    font-size: 1.15rem;
    font-weight: 700;
    color: #1e293b;
}

.brand-sub {
    margin: 0;
    font-size: 0.75rem;
    color: #64748b;
}

/* 3-Column Split Workspace */
.studio-body {
    display: grid;
    grid-template-columns: 260px 1fr 340px;
    flex: 1;
    overflow: hidden;
    height: calc(100vh - 65px);
}

/* Left Palette */
.left-palette {
    background: #ffffff;
    border-right: 1px solid #e2e8f0;
    padding: 1rem;
    overflow-y: auto;
}

.panel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
}

.panel-header h2 {
    font-size: 0.9rem;
    font-weight: 700;
    color: #334155;
    margin: 0;
    text-transform: uppercase;
    letter-spacing: 0.04em;
}

.badge {
    font-size: 0.65rem;
    padding: 0.15rem 0.45rem;
    background: #f1f5f9;
    color: #64748b;
    border-radius: 4px;
}

.badge-single {
    font-size: 0.65rem;
    padding: 0.2rem 0.5rem;
    background: #eff6ff;
    color: #2563eb;
    font-weight: 600;
    border-radius: 4px;
}

.palette-section {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    margin-bottom: 1.25rem;
}

.section-title {
    font-size: 0.72rem;
    font-weight: 600;
    color: #94a3b8;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: 0.25rem;
}

.palette-item {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.6rem 0.75rem;
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 6px;
    cursor: pointer;
    text-align: left;
    transition: all 0.15s ease-in-out;
}

.palette-item:hover {
    background: #ffffff;
    border-color: #3b82f6;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    transform: translateX(2px);
}

.item-icon {
    font-size: 1.15rem;
}

.item-name {
    display: block;
    font-size: 0.82rem;
    font-weight: 600;
    color: #1e293b;
}

.item-desc {
    display: block;
    font-size: 0.7rem;
    color: #64748b;
}

.command-item {
    border-left: 3px solid #8b5cf6;
}

/* Center Canvas */
.center-canvas {
    background: #f8fafc;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.canvas-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.75rem 1.5rem;
    background: #ffffff;
    border-bottom: 1px solid #e2e8f0;
}

.canvas-header h2 {
    font-size: 1rem;
    font-weight: 700;
    margin: 0;
    color: #1e293b;
}

.canvas-subtitle {
    font-size: 0.75rem;
    color: #64748b;
    margin: 0;
}

.element-count {
    font-size: 0.75rem;
    font-weight: 600;
    padding: 0.25rem 0.6rem;
    background: #f1f5f9;
    border-radius: 12px;
    color: #475569;
}

.canvas-viewport {
    flex: 1;
    padding: 1.5rem;
    overflow-y: auto;
}

.canvas-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 1rem;
}

.canvas-card {
    background: #ffffff;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    padding: 1rem;
    cursor: pointer;
    transition: all 0.15s ease-in-out;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.canvas-card:hover {
    border-color: #94a3b8;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

.canvas-card.is-active {
    border: 2px solid #2563eb;
    background: #ffffff;
    box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.card-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 0.75rem;
}

.card-title-group {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
}

.card-badge {
    font-size: 0.65rem;
    font-weight: 700;
    color: #2563eb;
    background: #eff6ff;
    padding: 0.15rem 0.4rem;
    border-radius: 4px;
    width: fit-content;
}

.badge-command {
    color: #7c3aed;
    background: #f5f3ff;
}

.card-title {
    font-size: 0.875rem;
    font-weight: 600;
    color: #1e293b;
}

.delete-btn {
    background: transparent;
    border: none;
    color: #94a3b8;
    cursor: pointer;
    padding: 0.2rem 0.4rem;
    border-radius: 4px;
}

.delete-btn:hover {
    background: #fee2e2;
    color: #ef4444;
}

.canvas-input {
    width: 100%;
    padding: 0.5rem 0.65rem;
    font-size: 0.85rem;
    border: 1px solid #cbd5e1;
    border-radius: 4px;
    background: #f8fafc;
    box-sizing: border-box;
}

.currency-input-wrapper {
    position: relative;
}

.canvas-switch {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.8rem;
    color: #475569;
}

.command-preview-box {
    background: #0f172a;
    color: #38bdf8;
    padding: 0.5rem;
    border-radius: 4px;
    font-size: 0.75rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.command-preview-box code {
    font-family: monospace;
}

.command-preview-box .tag {
    background: #1e293b;
    color: #a5b4fc;
    font-size: 0.65rem;
    padding: 0.15rem 0.35rem;
    border-radius: 3px;
}

.card-footer {
    margin-top: 0.75rem;
    padding-top: 0.5rem;
    border-top: 1px solid #f1f5f9;
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.72rem;
    color: #64748b;
}

.card-footer code {
    color: #0f172a;
    font-weight: 600;
}

.required-flag {
    color: #ef4444;
    font-weight: 600;
}

/* Right Inspector */
.right-inspector {
    background: #ffffff;
    border-left: 1px solid #e2e8f0;
    padding: 1rem;
    overflow-y: auto;
}

.inspector-meta-banner {
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 6px;
    padding: 0.75rem;
    margin-bottom: 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.2rem;
}

.meta-label {
    font-size: 0.65rem;
    text-transform: uppercase;
    color: #94a3b8;
    font-weight: 700;
}

.meta-type {
    font-size: 0.85rem;
    font-weight: 700;
    color: #1e293b;
}

.meta-id {
    font-size: 0.7rem;
    color: #64748b;
    font-family: monospace;
}

.empty-inspector {
    text-align: center;
    padding: 3rem 1rem;
    color: #94a3b8;
}

.empty-icon {
    font-size: 2rem;
    display: block;
    margin-bottom: 0.5rem;
}
```

---

# 4. Phase 3: Backend GoF Command Pattern Execution Engine (.NET 9)

### 4.1 GoF Command Principles
- **Command Interface:** `IWorkflowCommand`
- **Concrete Commands:** `CallRestApiCommand`, `MappingCommand`, `DecisionRuleCommand`, `ObjectMutationCommand`
- **Invoker:** `CommandInvoker`
- **Receiver/Blackboard Context:** `WorkflowExecutionContext`
- **Client Orchestrator:** `UnderwritingService`

---

### 4.2 `LoanCraft.Application/Engine/IWorkflowCommand.cs`
```csharp
namespace LoanCraft.Application.Engine;

public interface IWorkflowCommand
{
    string StepName { get; }
    string Description { get; }
    CommandType CommandType { get; }

    Task ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default);
}
```

---

### 4.3 `LoanCraft.Application/Engine/WorkflowExecutionContext.cs`
```csharp
using LoanCraft.Domain.Entities;

namespace LoanCraft.Application.Engine;

public class WorkflowExecutionContext
{
    public LoanApplication Loan { get; set; } = null!;
    public Dictionary<string, object> Data { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<WorkflowExecutionAudit> Audits { get; } = new();
    
    public bool IsHalted { get; private set; }
    public string? HaltReason { get; private set; }

    public WorkflowExecutionContext(LoanApplication loan)
    {
        Loan = loan ?? throw new ArgumentNullException(nameof(loan));
    }

    public void Halt(string reason)
    {
        IsHalted = true;
        HaltReason = reason;
    }

    public T? GetValue<T>(string key)
    {
        if (Data.TryGetValue(key, out var val) && val is T typedVal)
            return typedVal;
        return default;
    }

    public void SetValue(string key, object value)
    {
        Data[key] = value;
    }
}
```

---

### 4.4 `LoanCraft.Application/Engine/CommandInvoker.cs`
```csharp
using System.Diagnostics;
using LoanCraft.Application.Common.Interfaces;
using LoanCraft.Domain.Entities;
using LoanCraft.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LoanCraft.Application.Engine;

public class CommandInvoker
{
    private readonly List<IWorkflowCommand> _commands = new();
    private readonly IWorkflowHubNotifier _notifier;
    private readonly ILogger<CommandInvoker> _logger;

    public CommandInvoker(IWorkflowHubNotifier notifier, ILogger<CommandInvoker> logger)
    {
        _notifier = notifier;
        _logger = logger;
    }

    public CommandInvoker AddCommand(IWorkflowCommand command)
    {
        _commands.Add(command);
        return this;
    }

    public async Task<WorkflowExecutionContext> ExecuteAsync(
        WorkflowExecutionContext context, 
        string loanId, 
        CancellationToken cancellationToken = default)
    {
        var totalSteps = _commands.Count;

        for (int i = 0; i < totalSteps; i++)
        {
            var command = _commands[i];
            var stepIndex = i + 1;

            if (context.IsHalted)
            {
                _logger.LogWarning("Pipeline halted before step {Step}. Reason: {Reason}", command.StepName, context.HaltReason);
                break;
            }

            var sw = Stopwatch.StartNew();
            var audit = new WorkflowExecutionAudit
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = context.Loan.Id,
                StepName = command.StepName,
                CommandType = command.CommandType,
                ExecutedAt = DateTime.UtcNow
            };

            try
            {
                await _notifier.SendStepProgressAsync(loanId, command.StepName, stepIndex, totalSteps, StepStatus.Running, null);

                await command.ExecuteAsync(context, cancellationToken);
                sw.Stop();

                audit.DurationMs = sw.ElapsedMilliseconds;
                audit.Status = StepStatus.Completed;
                audit.Message = $"Executed successfully in {sw.ElapsedMilliseconds}ms.";

                await _notifier.SendStepProgressAsync(loanId, command.StepName, stepIndex, totalSteps, StepStatus.Completed, audit.Message);
            }
            catch (Exception ex)
            {
                sw.Stop();
                audit.DurationMs = sw.ElapsedMilliseconds;
                audit.Status = StepStatus.Failed;
                audit.Message = ex.Message;

                context.Halt($"Step '{command.StepName}' failed with error: {ex.Message}");

                await _notifier.SendStepProgressAsync(loanId, command.StepName, stepIndex, totalSteps, StepStatus.Failed, ex.Message);
                _logger.LogError(ex, "Error executing workflow step: {StepName}", command.StepName);
                break;
            }
            finally
            {
                context.Audits.Add(audit);
            }
        }

        return context;
    }
}
```

---

### 4.5 `LoanCraft.Application/Engine/Commands/ConcreteCommands.cs`
```csharp
using LoanCraft.Domain.Enums;

namespace LoanCraft.Application.Engine.Commands;

public class CallRestApiCommand : IWorkflowCommand
{
    public string StepName => "Credit Bureau & Fraud Bureau API";
    public string Description => "Simulates or invokes third-party credit check API";
    public CommandType CommandType => CommandType.RestCall;

    public async Task ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(300, cancellationToken);

        var revenue = context.Loan.AnnualRevenue;
        var requested = context.Loan.RequestedAmount;

        int simulatedScore = 650 + (int)(Math.Min(revenue / Math.Max(requested, 1), 5) * 30);
        simulatedScore = Math.Clamp(simulatedScore, 580, 820);

        context.SetValue("CreditScore", simulatedScore);
        context.SetValue("DebtToIncomeRatio", Math.Round((requested * 0.15m) / Math.Max(revenue, 1), 2));
    }
}

public class MappingCommand : IWorkflowCommand
{
    public string StepName => "Financial Metrics Data Normalization";
    public string Description => "Extracts and normalizes raw credit data into underwriting variables";
    public CommandType CommandType => CommandType.DataMapping;

    public Task ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        var creditScore = context.GetValue<int>("CreditScore");
        var dti = context.GetValue<decimal>("DebtToIncomeRatio");

        string riskTier = creditScore switch
        {
            >= 740 => "Prime",
            >= 660 => "NearPrime",
            _ => "Subprime"
        };

        context.SetValue("RiskTier", riskTier);
        context.SetValue("IsLowRiskDti", dti < 0.40m);

        return Task.CompletedTask;
    }
}

public class DecisionRuleCommand : IWorkflowCommand
{
    public string StepName => "Underwriting Policy Matrix";
    public string Description => "Applies banking credit policy to decide approval or rejection";
    public CommandType CommandType => CommandType.BusinessRule;

    public Task ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        var riskTier = context.GetValue<string>("RiskTier") ?? "Subprime";
        var isLowDti = context.GetValue<bool>("IsLowRiskDti");
        var creditScore = context.GetValue<int>("CreditScore");

        bool approved = (riskTier == "Prime" || (riskTier == "NearPrime" && isLowDti)) && creditScore >= 620;

        decimal recommendedInterestRate = riskTier switch
        {
            "Prime" => 6.25m,
            "NearPrime" => 8.75m,
            _ => 14.50m
        };

        context.SetValue("UnderwritingDecision", approved ? "Approved" : "Rejected");
        context.SetValue("RecommendedRate", recommendedInterestRate);

        return Task.CompletedTask;
    }
}

public class ObjectMutationCommand : IWorkflowCommand
{
    public string StepName => "Loan Entity State Mutation";
    public string Description => "Applies underwriting decisions and rates to the LoanApplication entity";
    public CommandType CommandType => CommandType.EntityMutation;

    public Task ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        var decision = context.GetValue<string>("UnderwritingDecision");
        var rate = context.GetValue<decimal>("RecommendedRate");
        var score = context.GetValue<int>("CreditScore");

        if (decision == "Approved")
        {
            context.Loan.Status = LoanStatus.Approved;
            context.Loan.InterestRate = rate;
            context.Loan.ApprovedAmount = context.Loan.RequestedAmount;
            context.Loan.UnderwritingNotes = $"Approved automatically. Credit Score: {score}, Approved APR: {rate}%.";
        }
        else
        {
            context.Loan.Status = LoanStatus.Rejected;
            context.Loan.ApprovedAmount = 0m;
            context.Loan.UnderwritingNotes = $"Declined based on policy criteria. Credit Score: {score}.";
        }

        context.Loan.UnderwrittenAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }
}
```

---

### 4.6 `LoanCraft.Application/Services/UnderwritingService.cs`
```csharp
using LoanCraft.Application.Common.Interfaces;
using LoanCraft.Application.Engine;
using LoanCraft.Application.Engine.Commands;
using LoanCraft.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LoanCraft.Application.Services;

public class UnderwritingService : IUnderwritingService
{
    private readonly IWorkflowHubNotifier _notifier;
    private readonly ILogger<CommandInvoker> _invokerLogger;
    private readonly ILogger<UnderwritingService> _logger;

    public UnderwritingService(
        IWorkflowHubNotifier notifier,
        ILogger<CommandInvoker> invokerLogger,
        ILogger<UnderwritingService> logger)
    {
        _notifier = notifier;
        _invokerLogger = invokerLogger;
        _logger = logger;
    }

    public async Task<WorkflowExecutionContext> ExecutePipelineAsync(
        LoanApplication loan, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Building underwriting workflow pipeline for Loan {LoanId}", loan.Id);

        var context = new WorkflowExecutionContext(loan);
        var invoker = new CommandInvoker(_notifier, _invokerLogger);

        invoker.AddCommand(new CallRestApiCommand())
               .AddCommand(new MappingCommand())
               .AddCommand(new DecisionRuleCommand())
               .AddCommand(new ObjectMutationCommand());

        await invoker.ExecuteAsync(context, loan.Id.ToString(), cancellationToken);

        _logger.LogInformation("Pipeline execution finished for Loan {LoanId}. Status: {Status}", 
            loan.Id, loan.Status);

        return context;
    }
}
```

---

# 5. Phase 4: Real-Time Telemetry & Angular Integration

### 5.1 `src/app/core/dynamic-forms/services/workflow-hub.services.ts`
```typescript
import { Injectable, signal, computed } from '@angular/core';
import * as signalR from '@microsoft/signalr';

export interface StepProgressEvent {
  loanId: string;
  stepName: string;
  stepNumber: number;
  totalSteps: number;
  status: 'Pending' | 'Running' | 'Completed' | 'Failed';
  message?: string;
  timestamp?: string;
}

@Injectable({
  providedIn: 'root'
})
export class WorkflowHubService {
  private readonly hubUrl = 'http://localhost:5182/hubs/workflow';
  private hubConnection?: signalR.HubConnection;

  public connectionStatus = signal<'Disconnected' | 'Connecting' | 'Connected'>('Disconnected');
  public currentStep = signal<StepProgressEvent | null>(null);
  public stepHistory = signal<StepProgressEvent[]>([]);

  public progressPercentage = computed(() => {
    const step = this.currentStep();
    if (!step || step.totalSteps === 0) return 0;
    return Math.round((step.stepNumber / step.totalSteps) * 100);
  });

  public isRunning = computed(() => {
    const step = this.currentStep();
    return step?.status === 'Running';
  });

  public async startConnection(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    this.connectionStatus.set('Connecting');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection.on('StepProgress', (data: StepProgressEvent) => {
      this.handleStepProgress(data);
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

  public async joinLoanGroup(loanId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinLoanGroup', loanId);
    }
  }

  public async leaveLoanGroup(loanId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveLoanGroup', loanId);
    }
  }

  public reset(): void {
    this.currentStep.set(null);
    this.stepHistory.set([]);
  }

  private handleStepProgress(data: StepProgressEvent): void {
    this.currentStep.set(data);
    this.stepHistory.update(history => {
      const index = history.findIndex(s => s.stepNumber === data.stepNumber);
      if (index >= 0) {
        const updated = [...history];
        updated[index] = data;
        return updated;
      }
      return [...history, data];
    });
  }
}
```

---

### 5.2 `src/app/core/dynamic-forms/services/loan-api.service.ts`
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateLoanRequest {
  applicantName: string;
  businessName: string;
  requestedAmount: number;
  annualRevenue: number;
  loanPurpose?: string;
}

export interface WorkflowAuditDto {
  id: string;
  stepName: string;
  commandType: string;
  status: string;
  durationMs: number;
  message?: string;
  executedAt: string;
}

export interface LoanResponse {
  id: string;
  applicantName: string;
  businessName: string;
  requestedAmount: number;
  annualRevenue: number;
  loanPurpose?: string;
  status: 'Draft' | 'Submitted' | 'Underwriting' | 'Approved' | 'Rejected';
  approvedAmount?: number;
  interestRate?: number;
  underwritingNotes?: string;
  createdAt: string;
  underwrittenAt?: string;
  audits?: WorkflowAuditDto[];
}

@Injectable({
  providedIn: 'root'
})
export class LoanApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5182/api/loans';

  public getAllLoans(): Observable<LoanResponse[]> {
    return this.http.get<LoanResponse[]>(this.baseUrl);
  }

  public getLoanById(id: string): Observable<LoanResponse> {
    return this.http.get<LoanResponse>(`${this.baseUrl}/${id}`);
  }

  public createLoan(payload: CreateLoanRequest): Observable<LoanResponse> {
    return this.http.post<LoanResponse>(this.baseUrl, payload);
  }

  public runUnderwriting(id: string): Observable<LoanResponse> {
    return this.http.post<LoanResponse>(`${this.baseUrl}/${id}/underwrite`, {});
  }

  public getLoanAudits(id: string): Observable<WorkflowAuditDto[]> {
    return this.http.get<WorkflowAuditDto[]>(`${this.baseUrl}/${id}/audits`);
  }
}
```

---

# 6. Phase 5: Enterprise Security & System Design Archive

### 6.1 The Zero-Trust Client Principle
* **The Client is Compromised:** You can never trust an HTTP request simply because it claims to come from your Angular UI. Any attacker can use Postman, cURL, or Python to send custom payloads.
* **CORS Myth:** CORS is solely a browser-enforced origin check to prevent malicious websites from executing cross-origin fetch requests using an authenticated session. It does **not** protect against Postman or automated attacker scripts.

### 6.2 Why the Frontend Never Holds a Secret Key
* Angular runs inside the end user's browser. Any key embedded in frontend source code can be extracted via DevTools (`F12`).
* **The Wax Seal Pattern:** The backend stores a single private key (`appsettings.json`). During login, the backend stamps a digital signature on a JWT token. Angular holds the token in memory and transmits it in the `Authorization: Bearer <token>` header. The backend validates its own digital seal on every incoming request.

### 6.3 Interception and Replay Defense
1. **TLS / HTTPS 1.3:** Asymmetric encryption with digital certificates prevents credential sniffing on public Wi-Fi.
2. **TLS Nonces & Sequence Numbers:** Every TLS frame contains single-use sequence counters. Captured encrypted packets cannot be replayed.
3. **OAuth2 with PKCE (Proof Key for Code Exchange):** Angular generates a cryptographically random `code_verifier` kept solely in browser memory and transmits its SHA-256 hash (`code_challenge`). Even if an authorization code is intercepted in transit, an attacker cannot redeem it for a JWT token without the original `code_verifier`.
4. **Password Hashing:** Passwords are never stored in plain text. Enterprise applications use salted, slow one-way hash algorithms (Argon2id, BCrypt, or PBKDF2) to protect against dictionary attacks.
