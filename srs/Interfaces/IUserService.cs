using srs.DTO;

namespace srs.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<IEnumerable<UserViewDTO>> GetAllUsersViewAsync();
        Task<UserDTO?> GetUserByIdAsync(int id);
        Task<UserDTO?> GetUserByEmailAsync(string email);
        Task<UserDTO> RegisterUserAsync(RegisterUserDTO registerUserDto, string userIP = "");
        Task<AuthResponseDTO?> LoginUserAsync(LoginUserDTO loginUserDto, string userIP = "");
        Task LogLogoutAsync(int userId, string userIP = "");
        Task UpdateUserAsync(int id, UpdateUserDTO updateUserDto);
        Task ChangePasswordAsync(int id, ChangePasswordDTO changePasswordDto);
        Task DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
    }
}
