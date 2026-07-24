using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain.Entities;
using TaskFlow.Api.Dtos;

namespace TaskFlow.Api.Services;

public class AuthService(TaskFlowDbContext db, TokenService tokenService)
{
    public async Task<AuthResponse> LoginAsync(AuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Email and password are required.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant());
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new ValidationException("Invalid email or password.");
        }

        return new AuthResponse(tokenService.CreateToken(user), MapUser(user));
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var displayName = request.DisplayName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        if (displayName.Length is < 2 or > 120)
        {
            throw new ValidationException("Display name must be between 2 and 120 characters.");
        }

        if (!MailAddress.TryCreate(email, out _))
        {
            throw new ValidationException("Enter a valid email address.");
        }

        if (password.Length < 8 ||
            !password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit))
        {
            throw new ValidationException(
                "Password must be at least 8 characters and include uppercase, lowercase, and a number.");
        }

        if (await db.Users.AnyAsync(user => user.Email == email))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            DisplayName = displayName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return new AuthResponse(tokenService.CreateToken(user), MapUser(user));
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found.");

        return MapUser(user);
    }

    public static UserDto MapUser(User user) => new(user.Id, user.Email, user.DisplayName);
}
