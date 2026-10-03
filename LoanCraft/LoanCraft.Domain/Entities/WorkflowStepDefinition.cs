using LoanCraft.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanCraft.Domain.Entities
{
    public class WorkflowStepDefinition
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowDefinitionId { get; set; }
        public int StepOrder { get; set; }
        public string StepKey { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public CommandType CommandType { get; set; }
        public string ConfigurationJson { get; set; } = "{}";
        public bool IsBreakpoint { get; set; } = false;
    }
}
