using app_baleares.Data;
using app_baleares.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCoreBackend.Models;
using NetCoreBackend.Models.Enum;
using NetCoreBackend.Services;

namespace NetCoreBackend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtHelper _jwtHelper;
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, IConfiguration configuration, JwtHelper jwtHelper, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _jwtHelper = jwtHelper;
            _emailService = emailService;
        }

        [HttpGet("users")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }


        [HttpPost("registerAPI")]
        public async Task<IActionResult> RegisterUserAPI(User user)
        {
            //Aqui agregue una logica diferente a la de Vistas, ya que aqui verifica si existe un usuario admin, en caso de que si predeterminadamente le asigna el rol REGULAR
            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Correo == user.Correo);

                if (existingUser != null)
                {
                    return BadRequest("El correo proporcionado ya está registrado.");
                }

                if (!_context.Users.Any())
                {
                    user.Rol = Rol.ADMIN;
                }
                else
                {
                    user.Rol = Rol.REGULAR;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { Id = user.Id, Nombre = user.Nombre, Correo = user.Correo, Avatar = user.Avatar });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar usuario: {ex.Message}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost("createAPI")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
        public async Task<IActionResult> Create(User user)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Correo == user.Correo);

                if (existingUser != null)
                {
                    return BadRequest("El correo proporcionado ya está registrado.");
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { Id = user.Id, Nombre = user.Nombre, Correo = user.Correo, Avatar = user.Avatar });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("edituserAPI/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
        public async Task<IActionResult> EditUserAPI(int id, User updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest("El ID del usuario no coincide.");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Correo == updatedUser.Correo && u.Id != id);

            if (existingUser != null)
            {
                return BadRequest("El correo proporcionado no cincide con los datos en la base de datos.");
            }

            try
            {
                _context.Entry(updatedUser).State = EntityState.Modified;

                if (!string.IsNullOrEmpty(updatedUser.Password))
                {
                    updatedUser.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
                }
                else
                {
                    _context.Entry(updatedUser).Property(u => u.Password).IsModified = false;
                }

                await _context.SaveChangesAsync();

                return Ok(new { Id = updatedUser.Id, Nombre = updatedUser.Nombre, Correo = updatedUser.Correo, Avatar = updatedUser.Avatar, });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(u => u.Id == id))
                {
                    return NotFound("Usuario no encontrado.");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return StatusCode(500, $"Error al actualizar el usuario: {ex.Message}");
            }
        }

        [HttpPut("editmyuserAPI/{id}")]
        public async Task<IActionResult> editMyUserAPI(int id, User updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest("El ID del usuario no coincide.");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Correo == updatedUser.Correo && u.Id != id);

            if (existingUser != null)
            {
                return BadRequest("El correo proporcionado ya está registrado.");
            }

            try
            {
                var userToUpdate = await _context.Users.FindAsync(id);
                if (userToUpdate == null)
                {
                    return NotFound("Usuario no encontrado.");
                }

                // Update only non-sensitive fields
                userToUpdate.Nombre = updatedUser.Nombre;
                userToUpdate.Correo = updatedUser.Correo;
                userToUpdate.Avatar = updatedUser.Avatar;

                // Save changes
                await _context.SaveChangesAsync();

                return Ok(new { Id = userToUpdate.Id, Nombre = userToUpdate.Nombre, Correo = userToUpdate.Correo, Avatar = userToUpdate.Avatar });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(u => u.Id == id))
                {
                    return NotFound("Usuario no encontrado.");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return StatusCode(500, $"Error al actualizar el usuario: {ex.Message}");
            }
        }

        [HttpDelete("deleteuserAPI/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Correo == request.Correo);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                    return Unauthorized();

                var token = _jwtHelper.GenerateToken(user);

                return Ok(new { Token = token, user = new { Id = user.Id, Nombre = user.Nombre, Correo = user.Correo, Avatar = user.Avatar, Rol = user.Rol } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging in: {ex.Message}");
                return StatusCode(500, ex); 
            }
        }

        [HttpGet("renew")]
        public IActionResult RenewToken([FromHeader(Name = "x-token")] string token)
        {
            try
            {
                var email = _jwtHelper.ExtractUsername(token);

                if (email == null)
                {
                    return Unauthorized("Token inválido");
                }

                if (_jwtHelper.ValidateToken(token, email))
                {
                    var user = _context.Users.SingleOrDefault(u => u.Correo == email);

                    if (user != null)
                    {
                        var newToken = _jwtHelper.GenerateToken(user);
                        return Ok(new { Token = token, user = new { Id = user.Id, Nombre = user.Nombre, Correo = user.Correo, Avatar = user.Avatar, Rol = user.Rol } });
                    }
                    else
                    {
                        return Unauthorized("Usuario no encontrado");
                    }
                }
                else
                {
                    return Unauthorized("Token inválido");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex); 
            }
        }

        [HttpGet("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string email)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Correo == email);

                if (existingUser == null)
                {
                    return BadRequest(new { Message = "El correo proporcionado no está registrado." });
                }

                var token = _jwtHelper.GeneratePasswordResetToken(existingUser);

                var callbackUrl = $"http://localhost:5173/auth/reset-password?token={token}&email={email}";
                var subject = "Resetear contraseña";
                var body = $"Para resetear tu contraseña, haz clic en el siguiente enlace: <a href='{callbackUrl}'>Resetear contraseña</a>";
                await _emailService.SendEmailAsync(email, subject, body);

                return Ok(new { Message = "Se ha enviado un correo electrónico con instrucciones para resetear tu contraseña." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                // Devuelve solo el mensaje de la excepción sin los detalles del método
                return StatusCode(500, new { Message = "Se ha producido un error al intentar resetear la contraseña." });
            }
        }

        [HttpPost("reset-password-confirm")]
        public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!_jwtHelper.ValidatePasswordResetToken(request.Token, request.Email))
                {
                    return BadRequest("Token inválido o expirado.");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Correo == request.Email);

                if (user == null)
                {
                    return BadRequest("El usuario no existe.");
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok("Contraseña reseteada exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return StatusCode(500, ex);
            }
        }
    }

    public class LoginRequest
    {
        public string Correo { get; set; }
        public string Password { get; set; }
    }
}
