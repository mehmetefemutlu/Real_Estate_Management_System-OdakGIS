using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using srs.DTO;
using srs.Entity;
using srs.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace srs.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogRepository _logRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, ILogRepository logRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _logRepository = logRepository;
            _configuration = configuration;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAll();
            return await Task.WhenAll(users.Select(MapToUserDTO));
        }

        public async Task<IEnumerable<UserViewDTO>> GetAllUsersViewAsync()
        {
            var users = await _userRepository.GetAll();
            return users.Select(u => new UserViewDTO
            {
                id = u.id,
                firstName = u.firstname,
                lastName = u.lastname,
                email = u.email,
                userRole = u.userRole
            });
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserWithProperties(id);
            if (user == null) return null;

            return await MapToUserDTO(user);
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null) return null;

            return await MapToUserDTO(user);
        }

        public async Task<UserDTO> RegisterUserAsync(RegisterUserDTO registerUserDto, string userIP = "")
        {
            ValidateRequired(registerUserDto.firstName, registerUserDto.lastName, registerUserDto.email);
            ValidatePassword(registerUserDto.password);

            var emailExists = await _userRepository.EmailExists(registerUserDto.email);
            if (emailExists)
                throw new InvalidOperationException($"Email {registerUserDto.email} is already registered");

            var passwordSalt = GenerateSalt();
            var passwordHash = HashPassword(registerUserDto.password, passwordSalt);

            var user = new User
            {
                firstname = registerUserDto.firstName,
                lastname = registerUserDto.lastName,
                email = registerUserDto.email,
                passwordHash = passwordHash,
                passwordSalt = passwordSalt,
                userRole = registerUserDto.userRole,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                properties = new List<Property>(),
                logs = new List<Log>()
            };

            var createdUser = await _userRepository.Add(user);
            await WriteLogAsync(
                createdUser.id,
                Log.OperationType.Create,
                "New registration",
                $"User {createdUser.firstname} {createdUser.lastname} registered with email {createdUser.email}.",
                userIP);
            return await MapToUserDTO(createdUser);
        }

        public async Task<AuthResponseDTO?> LoginUserAsync(LoginUserDTO loginUserDto, string userIP = "")
        {
            var user = await _userRepository.GetUserByEmail(loginUserDto.email);
            if (user == null)
                return null;

            if (!VerifyPassword(loginUserDto.password, user.passwordHash, user.passwordSalt))
                return null;

            await WriteLogAsync(
                user.id,
                Log.OperationType.Read,
                "User sign in",
                $"User {user.firstname} {user.lastname} signed in.",
                userIP);

            var token = GenerateJwtToken(user);
            return new AuthResponseDTO
            {
                userId = user.id,
                email = user.email,
                firstName = user.firstname,
                lastName = user.lastname,
                userRole = user.userRole,
                token = token
            };
        }

        public async Task LogLogoutAsync(int userId, string userIP = "")
        {
            var user = await _userRepository.GetById(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with id {userId} not found");

            await WriteLogAsync(
                user.id,
                Log.OperationType.Read,
                "User logout",
                $"User {user.firstname} {user.lastname} logged out.",
                userIP);
        }

        public async Task UpdateUserAsync(int id, UpdateUserDTO updateUserDto)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                throw new KeyNotFoundException($"User with id {id} not found");

            ValidateRequired(updateUserDto.firstName, updateUserDto.lastName, updateUserDto.email);

            user.firstname = updateUserDto.firstName!.Trim();
            user.lastname = updateUserDto.lastName!.Trim();

            var email = updateUserDto.email!.Trim();
            if (!string.Equals(user.email, email, StringComparison.OrdinalIgnoreCase))
            {
                var emailExists = await _userRepository.EmailExists(email);
                if (emailExists)
                    throw new InvalidOperationException($"Email {email} is already in use");

                user.email = email;
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.password))
            {
                ValidatePassword(updateUserDto.password);
                var newSalt = GenerateSalt();
                user.passwordSalt = newSalt;
                user.passwordHash = HashPassword(updateUserDto.password, newSalt);
            }

            user.userRole = updateUserDto.userRole;
            user.updatedAt = DateTime.UtcNow;
            await _userRepository.Update(user);
        }

        public async Task ChangePasswordAsync(int id, ChangePasswordDTO changePasswordDto)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                throw new KeyNotFoundException($"User with id {id} not found");

            if (!VerifyPassword(changePasswordDto.currentPassword, user.passwordHash, user.passwordSalt))
                throw new InvalidOperationException("Old password is incorrect");

            var newSalt = GenerateSalt();
            user.passwordSalt = newSalt;
            user.passwordHash = HashPassword(changePasswordDto.newPassword, newSalt);
            user.updatedAt = DateTime.UtcNow;

            await _userRepository.Update(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            var exists = await _userRepository.Exists(id);
            if (!exists)
                throw new KeyNotFoundException($"User with id {id} not found");

            await _userRepository.DeleteUserAndProperties(id);
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _userRepository.Exists(id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExists(email);
        }

        private async Task<UserDTO> MapToUserDTO(User user)
        {
            return new UserDTO
            {
                id = user.id,
                firstName = user.firstname,
                lastName = user.lastname,
                email = user.email,
                userRole = user.userRole,
                createdAt = user.createdAt,
                updatedAt = user.updatedAt,
                propertyCount = user.properties?.Count ?? 0
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Configure Jwt:Key using local settings or the Jwt__Key environment variable.");
            var issuer = _configuration["Jwt:Issuer"] ?? "SRSApi";
            var audience = _configuration["Jwt:Audience"] ?? "SRSClient";
            var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.email),
                new Claim(ClaimTypes.Role, user.userRole.ToString()),
                new Claim("userId", user.id.ToString()),
                new Claim("firstName", user.firstname),
                new Claim("lastName", user.lastname),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password, string salt)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(passwordBytes);
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var computedHash = HashPassword(password, storedSalt);
            return computedHash == storedHash;
        }

        private string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        private async Task WriteLogAsync(
            int userId,
            Log.OperationType operationType,
            string message,
            string description,
            string userIP)
        {
            await _logRepository.Add(new Log
            {
                userId = userId,
                operationType = operationType,
                message = message,
                description = description,
                status = Log.Status.Completed,
                userIP = userIP,
                createdAt = DateTime.UtcNow
            });
        }

        private static void ValidateRequired(params string?[] values)
        {
            if (values.Any(value => string.IsNullOrWhiteSpace(value)))
                throw new InvalidOperationException("Please check all required fields.");
        }

        private static void ValidatePassword(string password)
        {
            var isValid = password.Length is >= 8 and <= 12
                && Regex.IsMatch(password, "[A-Za-z]")
                && Regex.IsMatch(password, "[0-9]")
                && Regex.IsMatch(password, "[^A-Za-z0-9]");

            if (!isValid)
                throw new InvalidOperationException("Password must be between 8 and 12 characters and include at least one letter, one number, and one special character.");
        }
    }
}
