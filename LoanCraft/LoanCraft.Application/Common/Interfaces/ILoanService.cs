using LoanCraft.Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanCraft.Application.Common.Interfaces
{
    public interface ILoanService
    {
        Task<List<LoanResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<LoanResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Guid> CreateAsync(CreateLoanApplicationDto dto, CancellationToken cancellationToken = default);
    }
}
