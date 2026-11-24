using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using srs.DTO;
using srs.Interfaces;

namespace srs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> getAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<UserViewDTO>>> getAllUsersView()
        {
            var users = await _userService.GetAllUsersViewAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> getUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with id {id} not found" });

            return Ok(user);
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<UserDTO>> getUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(new { message = $"User with email {email} not found" });

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> registerUser([FromBody] RegisterUserDTO registerUserDto)
        {
            try
            {
                var user = await _userService.RegisterUserAsync(registerUserDto, GetClientIP());
                return CreatedAtAction(nameof(getUserById), new { id = user.id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> loginUser([FromBody] LoginUserDTO loginUserDto)
        {
            try
            {
                var response = await _userService.LoginUserAsync(loginUserDto, GetClientIP());
                if (response == null)
                    return Unauthorized(new { message = "Invalid email or password" });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> logoutUser()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                    return Unauthorized(new { message = "Invalid session" });

                await _userService.LogLogoutAsync(currentUserId.Value, GetClientIP());
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> updateUser(int id, [FromBody] UpdateUserDTO updateUserDto)
        {
            try
            {
                await _userService.UpdateUserAsync(id, updateUserDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/password")]
        public async Task<IActionResult> changePassword(int id, [FromBody] ChangePasswordDTO changePasswordDto)
        {
            try
            {
                await _userService.ChangePasswordAsync(id, changePasswordDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteUser(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                    return Unauthorized(new { message = "Invalid session" });

                if (currentUserId.Value != id && !User.IsInRole("Admin"))
                    return Forbid();

                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> userExists(int id)
        {
            var exists = await _userService.UserExistsAsync(id);
            return Ok(new { exists });
        }

        [HttpGet("exists/email/{email}")]
        public async Task<ActionResult<bool>> emailExists(string email)
        {
            var exists = await _userService.EmailExistsAsync(email);
            return Ok(new { exists });
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        private string GetClientIP()
        {
            return Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                ?? string.Empty;
        }
    }
}
