using LoanCraft.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanCraft.Domain.Entities
{
    public class WorkflowExecutionAudit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LoanApplicationId { get; set; }
        public string StepKey { get; set; } = string.Empty;
        public StepStatus Status { get; set; }
        public string? OutputSnapshotJson { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ExecutedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
