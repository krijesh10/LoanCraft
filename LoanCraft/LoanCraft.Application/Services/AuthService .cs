using LoanCraft.Application.Common.DTOs;
using LoanCraft.Application.Common.Interfaces;
using LoanCraft.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanCraft.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IApplicationDbContext context, IJwtTokenGenerator jwtGenerator, IPasswordHasher passwordHasher   )
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        var existing = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (existing) throw new InvalidOperationException("User with this email already exists.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = _passwordHasher.HashPassword(dto.Password), // Salted one-way hash
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Underwriter" : dto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtGenerator.GenerateToken(user);
        return new AuthResponseDto(token, user.FullName, user.Email, user.Role, DateTime.UtcNow.AddMinutes(120));
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = _jwtGenerator.GenerateToken(user);
        return new AuthResponseDto(token, user.FullName, user.Email, user.Role, DateTime.UtcNow.AddMinutes(120));
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new UnauthorizedAccessException("User session invalid.");

        var token = _jwtGenerator.GenerateToken(user);
        return new AuthResponseDto(token, user.FullName, user.Email, user.Role, DateTime.UtcNow.AddMinutes(120));
    }


}
