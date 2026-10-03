using System.Text.Json;
using LoanCraft.Domain.Entities;
using LoanCraft.Domain.Enums;
using LoanCraft.Application.Common.Interfaces;

namespace LoanCraft.Application.Engine.Commands;

// 1. External REST Call Command (Simulates Credit Bureau API / Salesforce sync)
public class CallRestApiCommand : IWorkflowCommand
{
    private readonly HttpClient _httpClient;
    public string StepKey { get; }
    public string DisplayName { get; }
    public string EndpointUrl { get; }

    public CallRestApiCommand(string stepKey, string displayName, string endpointUrl, HttpClient httpClient)
    {
        StepKey = stepKey;
        DisplayName = displayName;
        EndpointUrl = endpointUrl;
        _httpClient = httpClient;
    }

    public async Task<object?> ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
    {
        await Task.Delay(400, cancellationToken);
        var creditResult = new
        {
            BureauName = "Equifax/Experian Mock",
            ReportId = Guid.NewGuid().ToString("N")[..8],
            EstimatedScore = 745,
            Delinquencies = 0,
            Verified = true
        };
        context.SetVariable("CreditBureauResult", creditResult);
        return creditResult;
    }
}

// 2. Data Mapping Command (Calculates Debt-To-Income and Risk Metrics)
public class MappingCommand : IWorkflowCommand
{
    public string StepKey { get; }
    public string DisplayName { get; }

    public MappingCommand(string stepKey, string displayName)
    {
        StepKey = stepKey;
        DisplayName = displayName;
    }

    public Task<object?> ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
    {
        var loan = context.GetVariable<LoanApplication>("LoanApplication")
            ?? throw new InvalidOperationException("LoanApplication variable missing.");

        decimal monthlyDebt = loan.ExistingDebt;
        decimal dtiRatio = loan.MonthlyIncome > 0 ? (monthlyDebt / loan.MonthlyIncome) * 100 : 999;

        var calculation = new
        {
            DebtToIncomeRatio = Math.Round(dtiRatio, 2),
            MaxAllowedExposure = loan.MonthlyIncome * 36,
            CalculatedRiskTier = dtiRatio < 35 ? "Low" : dtiRatio < 50 ? "Medium" : "High"
        };

        context.SetVariable("RiskMetrics", calculation);
        return Task.FromResult<object?>(calculation);
    }
}

// 3. Decision Rule Command (Evaluates underwriting policies)
public class DecisionRuleCommand : IWorkflowCommand
{
    public string StepKey { get; }
    public string DisplayName { get; }

    public DecisionRuleCommand(string stepKey, string displayName)
    {
        StepKey = stepKey;
        DisplayName = displayName;
    }

    public Task<object?> ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
    {
        var loan = context.GetVariable<LoanApplication>("LoanApplication")!;
        
        bool meetsCreditCriteria = loan.CreditScore >= 680;
        bool approved = meetsCreditCriteria && loan.RequestedAmount <= (loan.MonthlyIncome * 40);

        var decision = new
        {
            IsApproved = approved,
            Recommendation = approved ? LoanStatus.Approved : LoanStatus.Rejected,
            Reason = approved 
                ? "Automatic Underwriting Criteria Met." 
                : "Insufficient Credit Profile or High DTI."
        };

        context.SetVariable("UnderwritingDecision", decision);
        return Task.FromResult<object?>(decision);
    }
}

// 4. Object Mutation Command (Updates domain entity and commits audit records)
public class ObjectMutationCommand : IWorkflowCommand
{
    private readonly IApplicationDbContext _dbContext;
    public string StepKey { get; }
    public string DisplayName { get; }

    public ObjectMutationCommand(string stepKey, string displayName, IApplicationDbContext dbContext)
    {
        StepKey = stepKey;
        DisplayName = displayName;
        _dbContext = dbContext;
    }

    public async Task<object?> ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
    {
        var loan = await _dbContext.LoanApplications.FindAsync(new object[] { context.LoanApplicationId }, cancellationToken);
        if (loan == null) return null;

        var decision = (dynamic)context.GetVariable<object>("UnderwritingDecision")!;
        loan.Status = decision.IsApproved ? LoanStatus.Approved : LoanStatus.Rejected;
        loan.UnderwriterNotes = decision.Reason;
        loan.UpdatedAtUtc = DateTime.UtcNow;

        _dbContext.WorkflowAudits.Add(new WorkflowExecutionAudit
        {
            LoanApplicationId = loan.Id,
            StepKey = StepKey,
            Status = StepStatus.Completed,
            OutputSnapshotJson = JsonSerializer.Serialize(decision),
            ExecutedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new { loan.Id, NewStatus = loan.Status.ToString(), loan.UnderwriterNotes };
    }
}