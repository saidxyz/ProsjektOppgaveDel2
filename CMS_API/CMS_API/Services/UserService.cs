using CMS_Project.Data;
using CMS_Project.Models.Entities;
using CMS_Project.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace CMS_Project.Services
{
    public class UserService : IUserService
    {
        private readonly CMSContext _context;
        private readonly IConfiguration _configuration;

        public UserService(CMSContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        
        /// Register user by Dto
        public async Task<User> RegisterUserAsync(RegisterDto registerDto)
        {
            // Sjekk om brukernavn eller e-post allerede finnes
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                throw new ArgumentException("Brukernavn eksisterer allerede.");

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                throw new ArgumentException("E-post eksisterer allerede.");

            // Opprett ny bruker
            var user = new User
            {
                Username = registerDto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Email = registerDto.Email,
                CreatedDate = DateTime.UtcNow,
                Documents = new List<Document>()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }


        /// check for username and verify the password. generates jwt and returns it
        public async Task<string> AuthenticateUserAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                throw new UnauthorizedAccessException("Ugyldig brukernavn eller passord.");

            // Generer JWT-token
            var token = GenerateJwtToken(user);
            return token;
        }
        
        /// Generates JWT token by given user
        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("JWT Key er ikke konfigurert.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        /// Find user by username and returns id. if failed returns -1
        public async Task<int> GetUserIdAsync(string username)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null)
                throw new ArgumentException("User not found.");

            return user.Id;
        }
        
        
        public async Task<int> GetUserIdFromClaimsAsync(ClaimsPrincipal user)
        {
            var claimsIdentity = (ClaimsIdentity)user.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var username = claims.Value;

            return await GetUserIdAsync(username);
        }
        
        /// Retrieves a user by their unique identifier.
        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new ArgumentException("User not found.");

            return user;
        }

        public async Task<UserDto?> GetUserDtoByIdAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedDate = user.CreatedDate
            };
        }

    }
}
