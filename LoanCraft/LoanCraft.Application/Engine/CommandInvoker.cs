using Microsoft.Extensions.Logging;
using LoanCraft.Application.Common.Interfaces;

namespace LoanCraft.Application.Engine;

public class CommandInvoker
{
    private readonly List<IWorkflowCommand> _commands = new();
    private readonly IWorkflowHubNotifier _hubNotifier;
    private readonly ILogger<CommandInvoker> _logger;

    public CommandInvoker(IWorkflowHubNotifier hubNotifier, ILogger<CommandInvoker> logger)
    {
        _hubNotifier = hubNotifier;
        _logger = logger;
    }

    public void AddCommand(IWorkflowCommand command) => _commands.Add(command);

    public async Task ExecutePipelineAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        int total = _commands.Count;
        for (int i = 0; i < total; i++)
        {
            var command = _commands[i];
            try
            {
                _logger.LogInformation("Executing pipeline step [{Index}/{Total}]: {StepKey}", i + 1, total, command.StepKey);

                await _hubNotifier.NotifyStepStartedAsync(context.LoanApplicationId, command.StepKey, command.DisplayName, i + 1, total);

                var result = await command.ExecuteAsync(context, cancellationToken);
                if (result != null)
                {
                    context.StoreStepOutput(command.StepKey, result);
                }

                await _hubNotifier.NotifyStepCompletedAsync(context.LoanApplicationId, command.StepKey, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Step {StepKey} failed: {Message}", command.StepKey, ex.Message);
                await _hubNotifier.NotifyStepFailedAsync(context.LoanApplicationId, command.StepKey, ex.Message);
                throw;
            }
        }
    }
}