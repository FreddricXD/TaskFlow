using Microsoft.EntityFrameworkCore;
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

    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found.");

        return MapUser(user);
    }

    public static UserDto MapUser(User user) => new(user.Id, user.Email, user.DisplayName);
}
