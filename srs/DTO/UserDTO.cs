using srs.Entity;

namespace srs.DTO
{
    public class UserDTO
    {
        public int id { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string fullName => $"{firstName} {lastName}";
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public User.Role userRole { get; set; }
        public int propertyCount { get; set; }
    }
    public class UserViewDTO
    {
        public int id { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public User.Role userRole { get; set; }
    }
    public class RegisterUserDTO
    {
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public User.Role userRole { get; set; }
    }

    public class LoginUserDTO
    {
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
    public class UpdateUserDTO
    {
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public User.Role userRole { get; set; }
    }
    public class ChangePasswordDTO
    {
        public string currentPassword { get; set; } = string.Empty;
        public string newPassword { get; set; } = string.Empty;
    }
    public class AuthResponseDTO
    {
        public int userId { get; set; }
        public string email { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public User.Role userRole { get; set; }
        public string token { get; set; } = string.Empty;
    }
}

