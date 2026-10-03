using LoanCraft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LoanCraft.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<LoanApplication> LoanApplications { get; }
    DbSet<WorkflowDefinition> WorkflowDefinitions { get; }
    DbSet<WorkflowStepDefinition> WorkflowSteps { get; }
    DbSet<WorkflowExecutionAudit> WorkflowAudits { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
