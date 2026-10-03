namespace LoanCraft.Application.Common.DTOs;

public record RegisterRequestDto(string FullName, string Email, string Password, string Role);
public record LoginRequestDto(string Email, string Password);
public record AuthResponseDto(string Token, string FullName, string Email, string Role, DateTime ExpiresAtUtc);
