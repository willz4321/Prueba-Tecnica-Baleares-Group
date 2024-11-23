using app_baleares.Data;
using app_baleares.Helpers;
using Azure.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCoreBackend.Models;
using NetCoreBackend.Models.Enum;
using NetCoreBackend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace NetCoreBackend.Controllers
{
    public class AuthViewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtHelper _jwtHelper;
        private readonly IEmailService _emailService;

        public AuthViewController(AppDbContext context, IConfiguration configuration, JwtHelper jwtHelper, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _jwtHelper = jwtHelper;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpGet("register")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return View();
            }
            return RedirectToAction("Usuarios");

        }

        [HttpGet("createUser")]
        public IActionResult CreateUser()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
               return View();
        }

        [HttpGet("Reloaded")]
        public IActionResult RediretLogin()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("", "AuthView");
        }

        [HttpGet("perfil")]
        public IActionResult Perfil()
        {
            if (User.Identity!.IsAuthenticated)
            {
               return View();
            }
            return RedirectToAction("");
        }

        [HttpGet("usuarios")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Usuarios()
        {
            if (User.Identity!.IsAuthenticated)
            {
                try
                {
                    var usuarios = await _context.Users.ToListAsync();

                    return View(usuarios);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error al consultar datos: {ex.Message}");
                    ViewData["Error"] = "Datos incorrectos: " + ex.Message;
                    return View();
                }

            }
                return View("");
        }

        [HttpGet("reset-password")]
        public IActionResult ResetPasswordView()
        {
            return View("ResetPassword");
        }

        [HttpPost("register")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Register(User user, IFormFile Avatar)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Correo == user.Correo);
                if (existingUser != null)
                {
                    ViewData["Error"] = "El correo proporcionado ya está registrado.";
                    return View("register");
                }

                //Guardo la foto, creo la carpeta, y asigno el nombre al atributo
                if (Avatar != null && Avatar.Length > 0)
                {
                    var fileName = Path.GetFileName(Avatar.FileName);
                    var filePath = Path.Combine("wwwroot/media/users", fileName);

                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Avatar.CopyToAsync(stream);
                    }

                    user.Avatar = $"/media/users/{fileName}";
                }
                else
                {
                    user.Avatar = "/media/users/usuario_default.png";
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Rol = user.Rol;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Usuarios", "AuthView");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar usuario: {ex.Message}");
                ViewData["Error"] = "Datos incorrectos:" + ex.Message;
                return View("register");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user, IFormFile Avatar)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Correo == user.Correo);

                if (existingUser != null)
                {
                    ViewData["Error"] = "El correo proporcionado ya está registrado.";
                    return View("register");
                }

                //Guardo la foto, creo la carpeta, y asigno el nombre al atributo
                if (Avatar != null && Avatar.Length > 0)
                {
                    var fileName = Path.GetFileName(Avatar.FileName);
                    var filePath = Path.Combine("wwwroot/media/users", fileName);

                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Avatar.CopyToAsync(stream);
                    }

                    user.Avatar = $"/media/users/{fileName}";
                }
                else
                {
                    user.Avatar = "/media/users/usuario_default.png";
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Rol =  Rol.REGULAR;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Usuarios", "AuthView");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar usuario: {ex.Message}");
                ViewData["Error"] = "Datos incorrectos:" + ex.Message;
                return View("register");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string Correo, string Password)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Correo == Correo);

                if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.Password))
                {
                    ViewData["Error"] = "Credenciales invalidas";
                    Console.WriteLine("Error de login: Credenciales incorrectas");
                    return View();
                }

                var token = _jwtHelper.GenerateToken(user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Nombre),
                    new Claim(ClaimTypes.Email, user.Correo),
                    new Claim("Avatar", user.Avatar),
                    new Claim(ClaimTypes.Role, user.Rol.ToString()),
                    new Claim("Id", user.Id.ToString()),

                };

                var identity = new ClaimsIdentity(claims, "Login");

                var principal = new ClaimsPrincipal(identity);

                Response.Cookies.Append("x-token", token, new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(20)
                });

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de login: {ex.Message}");
                ViewData["Error"] = "Error al iniciar sesión: " + ex.Message;
                return View();
            }
        }

        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("", "AuthView");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditPerfile(User user, IFormFile Avatar)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Correo == user.Correo && u.Id != user.Id);

                if (existingUser != null)
                {
                    ViewData["Error"] = "El correo proporcionado ya está registrado, no lo puede usar!";
                    return View("perfil");
                }

                var userToUpdate = await _context.Users.FindAsync(user.Id);
                if (userToUpdate == null)
                {
                    ViewData["Error"] = "Usuario no encontrado.";
                    return View("perfil");
                }

                // Guardo la foto, creo la carpeta, y asigno el nombre al atributo
                if (Avatar != null && Avatar.Length > 0)
                {
                    var fileName = Path.GetFileName(Avatar.FileName);
                    var filePath = Path.Combine("wwwroot/media/users", fileName);

                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Avatar.CopyToAsync(stream);
                    }

                    userToUpdate.Avatar = $"/media/users/{fileName}";
                }

                userToUpdate.Nombre = user.Nombre;
                userToUpdate.Correo = user.Correo;
                userToUpdate.Rol = user.Rol;

                _context.Users.Update(userToUpdate);
                await _context.SaveChangesAsync();

                var token = _jwtHelper.GenerateToken(userToUpdate);

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userToUpdate.Nombre ?? string.Empty),
            new Claim(ClaimTypes.Email, userToUpdate.Correo ?? string.Empty),
            new Claim("Avatar", userToUpdate.Avatar ?? string.Empty),
            new Claim(ClaimTypes.Role, userToUpdate.Rol.ToString()),
            new Claim("Id", user.Id.ToString()),
        };

                var identity = new ClaimsIdentity(claims, "Login");
                var principal = new ClaimsPrincipal(identity);

                Response.Cookies.Append("x-token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(20)
                });

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("perfil");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar usuario: {ex.Message}");
                ViewData["Error"] = "Datos incorrectos: " + ex.Message;
                return View("perfil");
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> OrderEmail()
        {
            try
            {
                var usuariosOrdenados = await _context.Users
                    .OrderBy(u => u.Correo)
                    .ToListAsync();

                return View("usuarios", usuariosOrdenados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al consultar usuarios: {ex.Message}\n{ex.StackTrace}");
                ViewData["Error"] = "Ocurrió un error al cargar los usuarios. Intente nuevamente.";
                return RedirectToAction("Usuarios");
            }
        }
    }
}
