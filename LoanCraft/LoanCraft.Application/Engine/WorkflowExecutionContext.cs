using LoanCraft.Domain.Entities;

namespace LoanCraft.Application.Engine;

public class WorkflowExecutionContext
{
    public Guid LoanApplicationId { get; set; }
    public Dictionary<string, object> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, object> StepOutputs { get; } = new(StringComparer.OrdinalIgnoreCase);

    public void SetVariable(string key, object value) => Variables[key] = value;
    
    public T? GetVariable<T>(string key)
    {
        if (Variables.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return default;
    }

    public void StoreStepOutput(string stepKey, object output) => StepOutputs[stepKey] = output;
}



//public class WorkflowExecutionContext
//{
//    public LoanApplication Loan { get; set; } = null!;
//    public Dictionary<string, object> Data { get; } = new(StringComparer.OrdinalIgnoreCase);
//    public List<WorkflowExecutionAudit> Audits { get; } = new();

//    public bool IsHalted { get; private set; }
//    public string? HaltReason { get; private set; }

//    public WorkflowExecutionContext(LoanApplication loan)
//    {
//        Loan = loan ?? throw new ArgumentNullException(nameof(loan));
//    }

//    public void Halt(string reason)
//    {
//        IsHalted = true;
//        HaltReason = reason;
//    }

//    public T? GetValue<T>(string key)
//    {
//        if (Data.TryGetValue(key, out var val) && val is T typedVal)
//            return typedVal;
//        return default;
//    }

//    public void SetValue(string key, object value)
//    {
//        Data[key] = value;
//    }
//}
