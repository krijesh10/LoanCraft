using System.Net.Http;
using Microsoft.Extensions.Logging;
using LoanCraft.Application.Common.Interfaces;
using LoanCraft.Application.Engine;
using LoanCraft.Application.Engine.Commands;
using LoanCraft.Domain.Entities;
using LoanCraft.Domain.Enums;

namespace LoanCraft.Application.Services;

public interface IUnderwritingService
{
    Task<bool> ExecuteUnderwritingPipelineAsync(Guid loanId, CancellationToken cancellationToken = default);
}

public class UnderwritingService : IUnderwritingService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IWorkflowHubNotifier _hubNotifier;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CommandInvoker> _invokerLogger;

    public UnderwritingService(
        IApplicationDbContext dbContext,
        IWorkflowHubNotifier hubNotifier,
        IHttpClientFactory httpClientFactory,
        ILogger<CommandInvoker> invokerLogger)
    {
        _dbContext = dbContext;
        _hubNotifier = hubNotifier;
        _httpClientFactory = httpClientFactory;
        _invokerLogger = invokerLogger;
    }

    public async Task<bool> ExecuteUnderwritingPipelineAsync(Guid loanId, CancellationToken cancellationToken = default)
    {
        var loan = await _dbContext.LoanApplications.FindAsync(new object[] { loanId }, cancellationToken);
        if (loan == null) throw new KeyNotFoundException($"LoanApplication {loanId} not found.");

        // 1. Prepare Execution Context
        var context = new WorkflowExecutionContext { LoanApplicationId = loan.Id };
        context.SetVariable("LoanApplication", loan);

        // 2. Assemble Command Pipeline
        var invoker = new CommandInvoker(_hubNotifier, _invokerLogger);
        var httpClient = _httpClientFactory.CreateClient("ExternalApi");

        // Execute the 4 core pipeline commands:
        invoker.AddCommand(new CallRestApiCommand("STEP_BUREAU_PULL", "External Credit Bureau Query", "https://api.mock.credit/score", httpClient));
        invoker.AddCommand(new MappingCommand("STEP_DTI_MAPPING", "Financial Exposure & DTI Mapping"));
        invoker.AddCommand(new DecisionRuleCommand("STEP_UNDERWRITE_DECISION", "Automated Policy Decision Evaluation"));
        invoker.AddCommand(new ObjectMutationCommand("STEP_PERSIST_RESULTS", "Persist Underwriting Results & Audit", _dbContext));

        // 3. Execute Pipeline
        await invoker.ExecutePipelineAsync(context, cancellationToken);

        await _hubNotifier.NotifyWorkflowCompletedAsync(loan.Id, loan.Status.ToString(), "Underwriting pipeline executed successfully.");
        return true;
    }
}