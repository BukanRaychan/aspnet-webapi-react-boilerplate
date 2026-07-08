using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WebApi.DTOs.AuthDtos;
using WebApi.Models;

namespace WebApi.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null) {
            return null;
        }

        ApplicationUser user = new()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        IdentityResult result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) {
            return null;
        }

        return GenerateToken(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) {
            return null;
        }

        bool passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid) {
            return null;
        }

        return GenerateToken(user);
    }

    public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        if (dto.CurrentPassword != null && dto.NewPassword != null)
        {
            IdentityResult passwordResult = await _userManager.ChangePasswordAsync(
                user,
                dto.CurrentPassword,
                dto.NewPassword
            );
            if (!passwordResult.Succeeded) return false;
        }

        IdentityResult updateResult = await _userManager.UpdateAsync(user);
        return updateResult.Succeeded;
    }

    private AuthResponseDto GenerateToken(ApplicationUser user)
    {
        string key = _configuration["Jwt:Key"]!;
        string issuer = _configuration["Jwt:Issuer"]!;
        string audience = _configuration["Jwt:Audience"]!;
        DateTime expiresAt = DateTime.UtcNow.AddHours(24);

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
        ];

        SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(key));
        SigningCredentials credentials = new(signingKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ExpiresAt = expiresAt
        };
    }
}
