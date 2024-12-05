using CMS_Project.Models.Entities;
using CMS_Project.Models.DTOs;
using System.Security.Claims;

namespace CMS_Project.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserDtoByIdAsync(int userId);
        Task<User> RegisterUserAsync(RegisterDto registerDto);
        Task<string> AuthenticateUserAsync(LoginDto loginDto);
        Task<int> GetUserIdAsync(string username);
        Task<int> GetUserIdFromClaimsAsync(ClaimsPrincipal user);
        Task<User> GetUserByIdAsync(int userId);
    }
}