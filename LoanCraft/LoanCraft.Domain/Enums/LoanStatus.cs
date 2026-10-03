using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanCraft.Domain.Enums
{
    public enum LoanStatus
    {
        Draft = 0,
        Submitted = 1,
        UnderwritingInProgress = 2,
        Approved = 3,
        Rejected = 4,
        ManualReviewRequired = 5
    }
    public enum CommandType
    {
        CallRestApi = 1,
        DataMapping = 2,
        DecisionRule = 3,
        DocumentConversion = 4,
        ObjectMutation = 5
    }
    public enum StepStatus
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        PausedAtBreakpoint = 4
    }

}
