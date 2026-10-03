namespace LoanCraft.Application.Engine;

public interface IWorkflowCommand
{
    string StepKey { get; }
    string DisplayName { get; }
    Task<object?> ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken);
}