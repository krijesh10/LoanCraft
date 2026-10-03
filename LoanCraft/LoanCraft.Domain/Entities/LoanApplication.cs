using LoanCraft.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanCraft.Domain.Entities
{

    public class LoanApplication
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ApplicationNumber { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public int TenureMonths { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal ExistingDebt { get; set; }
        public int CreditScore { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.Draft;
        public string? UnderwriterNotes { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }

}
