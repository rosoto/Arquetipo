using Arquetipo.Api.Infrastructure;
using Arquetipo.Api.Models.Request;
using Arquetipo.Api.Models.Response;
using Arquetipo.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arquetipo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController(
        IUsuarioRepository usuarioRepository,
        ITokenService tokenService,
        ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails { Title = "Solicitud inválida", Detail = "El modelo de login no es válido.", Status = StatusCodes.Status400BadRequest });
            }

            _logger.LogInformation("Intento de login para el usuario: {NombreUsuario}", loginRequest.NombreUsuario);
            var usuario = await _usuarioRepository.GetUsuarioPorNombreAsync(loginRequest.NombreUsuario);

            if (usuario == null || !usuario.EstaActivo)
            {
                _logger.LogWarning("Login fallido: Usuario {NombreUsuario} no encontrado o inactivo.", loginRequest.NombreUsuario);
                return Unauthorized(new ProblemDetails { Title = "Error de autenticación", Detail = "Usuario o contraseña incorrectos.", Status = StatusCodes.Status401Unauthorized });
            }

            var tokenString = _tokenService.GenerarToken(usuario);
            _logger.LogInformation("Login exitoso y token generado para el usuario: {NombreUsuario}", loginRequest.NombreUsuario);

            return Ok(new LoginResponse
            {
                Token = tokenString,
                Expiration = DateTime.UtcNow.AddHours(1),
                NombreUsuario = usuario.NombreUsuario
            });
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails { Title = "Solicitud inválida", Detail = "El modelo de registro no es válido.", Status = StatusCodes.Status400BadRequest });
            }

            _logger.LogInformation("Intento de registro para el usuario: {NombreUsuario}", registerRequest.NombreUsuario);
            try
            {
                var nuevoUsuario = new Usuario
                {
                    NombreUsuario = registerRequest.NombreUsuario,
                    PasswordHash = registerRequest.Password,
                    Roles = registerRequest.Roles,
                    EstaActivo = true
                };

                var usuarioCreado = await _usuarioRepository.AddUsuarioAsync(nuevoUsuario, registerRequest.Password);


                return CreatedAtAction(nameof(Login), new { },
                                       new { Message = "Usuario registrado exitosamente.", UsuarioId = usuarioCreado.Id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registro fallido: El nombre de usuario {NombreUsuario} ya existe.", registerRequest.NombreUsuario);
                return Conflict(new ProblemDetails { Title = "Conflicto", Detail = ex.Message, Status = StatusCodes.Status409Conflict });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante el registro del usuario {NombreUsuario}.", registerRequest.NombreUsuario);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  new ProblemDetails { Title = "Error interno", Detail = "Ocurrió un error durante el registro.", Status = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
