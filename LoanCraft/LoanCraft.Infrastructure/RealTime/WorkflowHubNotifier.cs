using LoanCraft.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanCraft.Infrastructure.RealTime
{
    public class WorkflowHubNotifier : IWorkflowHubNotifier
    {
        private readonly IHubContext<WorkflowHub> _hubContext;
        public WorkflowHubNotifier(IHubContext<WorkflowHub> hubContext) => _hubContext = hubContext;
        public async Task NotifyStepStartedAsync(Guid loanId, string stepKey, string stepName, int stepIndex, int totalSteps)
        {
            await _hubContext.Clients.Group($"loan-{loanId}").SendAsync("StepStarted", new
            {
                LoanId = loanId,
                StepKey = stepKey,
                StepName = stepName,
                StepIndex = stepIndex,
                TotalSteps = totalSteps,
                Timestamp = DateTime.UtcNow
            });
        }
        public async Task NotifyStepCompletedAsync(Guid loanId, string stepKey, object? stepResult)
        {
            await _hubContext.Clients.Group($"loan-{loanId}").SendAsync("StepCompleted", new
            {
                LoanId = loanId,
                StepKey = stepKey,
                Result = stepResult,
                Timestamp = DateTime.UtcNow
            });
        }
        public async Task NotifyStepFailedAsync(Guid loanId, string stepKey, string error)
        {
            await _hubContext.Clients.Group($"loan-{loanId}").SendAsync("StepFailed", new
            {
                LoanId = loanId,
                StepKey = stepKey,
                Error = error,
                Timestamp = DateTime.UtcNow
            });
        }
        public async Task NotifyWorkflowCompletedAsync(Guid loanId, string finalStatus, string summary)
        {
            await _hubContext.Clients.Group($"loan-{loanId}").SendAsync("WorkflowCompleted", new
            {
                LoanId = loanId,
                Status = finalStatus,
                Summary = summary,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
