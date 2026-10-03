using LoanCraft.Application.Common.DTOs;
using LoanCraft.Application.Common.Interfaces;
using LoanCraft.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoanCraft.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly IUnderwritingService _underwritingService;

    public LoansController(ILoanService loanService, IUnderwritingService underwritingService)
    {
        _loanService = loanService;
        _underwritingService = underwritingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _loanService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var loan = await _loanService.GetByIdAsync(id);
        return loan == null ? NotFound() : Ok(loan);
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] CreateLoanApplicationDto dto)
    {
        var loanId = await _loanService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = loanId }, new { LoanId = loanId });
    }

    [HttpPost("{id:guid}/execute-underwriting")]
    public async Task<IActionResult> ExecuteUnderwriting(Guid id)
    {
        var success = await _underwritingService.ExecuteUnderwritingPipelineAsync(id);
        return Ok(new { Success = success, Message = "Underwriting pipeline completed successfully." });
    }
}
