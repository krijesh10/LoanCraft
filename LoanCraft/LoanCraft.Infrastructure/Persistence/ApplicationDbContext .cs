using LoanCraft.Application.Common.Interfaces;
using LoanCraft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanCraft.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public DbSet<User> Users => Set<User>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
        public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
        public DbSet<WorkflowStepDefinition> WorkflowSteps => Set<WorkflowStepDefinition>();
        public DbSet<WorkflowExecutionAudit> WorkflowAudits => Set<WorkflowExecutionAudit>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LoanApplication>().Property(x => x.RequestedAmount).HasPrecision(18, 2);
            modelBuilder.Entity<LoanApplication>().Property(x => x.MonthlyIncome).HasPrecision(18, 2);
            modelBuilder.Entity<LoanApplication>().Property(x => x.ExistingDebt).HasPrecision(18, 2);
        }
    }

}
