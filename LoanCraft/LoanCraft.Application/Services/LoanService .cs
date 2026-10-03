using Microsoft.EntityFrameworkCore;
using LoanCraft.Application.Common.DTOs;
using LoanCraft.Application.Common.Interfaces;
using LoanCraft.Domain.Entities;
using LoanCraft.Domain.Enums;

namespace LoanCraft.Application.Services;

public class LoanService : ILoanService
{
    private readonly IApplicationDbContext _context;
    
    public LoanService(IApplicationDbContext context) => _context = context;

    public async Task<List<LoanResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LoanApplications
            .Select(x => new LoanResponseDto(
                x.Id,
                x.ApplicationNumber,
                x.ApplicantName,
                x.ApplicantEmail,
                x.RequestedAmount,
                x.TenureMonths,
                x.MonthlyIncome,
                x.ExistingDebt,
                x.CreditScore,
                x.Status.ToString(),
                x.UnderwriterNotes,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<LoanResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var x = await _context.LoanApplications.FindAsync(new object[] { id }, cancellationToken);
        if (x == null) return null;

        return new LoanResponseDto(
            x.Id,
            x.ApplicationNumber,
            x.ApplicantName,
            x.ApplicantEmail,
            x.RequestedAmount,
            x.TenureMonths,
            x.MonthlyIncome,
            x.ExistingDebt,
            x.CreditScore,
            x.Status.ToString(),
            x.UnderwriterNotes,
            x.CreatedAtUtc);
    }

    public async Task<Guid> CreateAsync(CreateLoanApplicationDto dto, CancellationToken cancellationToken = default)
    {
        var loan = new LoanApplication
        {
            ApplicationNumber = $"LN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}",
            ApplicantName = dto.ApplicantName,
            ApplicantEmail = dto.ApplicantEmail,
            RequestedAmount = dto.RequestedAmount,
            TenureMonths = dto.TenureMonths,
            MonthlyIncome = dto.MonthlyIncome,
            ExistingDebt = dto.ExistingDebt,
            CreditScore = dto.CreditScore,
            Status = LoanStatus.Submitted
        };

        _context.LoanApplications.Add(loan);
        await _context.SaveChangesAsync(cancellationToken);
        return loan.Id;
    }
}