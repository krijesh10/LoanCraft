using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanCraft.Application.Common.Interfaces
{
    public interface IWorkflowHubNotifier
    {
        Task NotifyStepStartedAsync(Guid loanId, string stepKey, string stepName, int stepIndex, int totalSteps);
        Task NotifyStepCompletedAsync(Guid loanId, string stepKey, object? stepResult);
        Task NotifyStepFailedAsync(Guid loanId, string stepKey, string error);
        Task NotifyWorkflowCompletedAsync(Guid loanId, string finalStatus, string summary);
    }
}
