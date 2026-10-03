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
