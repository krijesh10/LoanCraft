using Microsoft.AspNetCore.SignalR;

namespace LoanCraft.Infrastructure.RealTime
{
    public class WorkflowHub : Hub
    {
        public async Task JoinLoanExecutionGroup(string loanId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"loan-{loanId}");
        }
    }
}