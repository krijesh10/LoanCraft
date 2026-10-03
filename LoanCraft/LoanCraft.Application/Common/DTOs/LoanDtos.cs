namespace LoanCraft.Application.Common.DTOs
{
    public record CreateLoanApplicationDto(
     string ApplicantName,
     string ApplicantEmail,
     decimal RequestedAmount,
     int TenureMonths,
     decimal MonthlyIncome,
     decimal ExistingDebt,
     int CreditScore);
    public record LoanResponseDto(
        Guid Id,
        string ApplicationNumber,
        string ApplicantName,
        string ApplicantEmail,
        decimal RequestedAmount,
        int TenureMonths,
        decimal MonthlyIncome,
        decimal ExistingDebt,
        int CreditScore,
        string Status,
        string? UnderwriterNotes,
        DateTime CreatedAtUtc);

}
