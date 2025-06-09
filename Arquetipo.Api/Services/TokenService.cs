using Arquetipo.Api.Models.Response;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Arquetipo.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            // var audience = jwtSettings["Audience"]; // Si tienes una audiencia específica

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer))
            {
                throw new InvalidOperationException("Configuración de JWT (Key, Issuer) no encontrada o incompleta.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.NombreUsuario), // Subject (username)
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()) // User ID
            // Puedes añadir más claims, como roles, si los tienes
        };

            if (!string.IsNullOrEmpty(usuario.Roles))
            {
                var roles = usuario.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var rol in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol.Trim()));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Duración del token (ej. 1 hora)
                Issuer = issuer,
                // Audience = audience, // Descomenta si usas Audience
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
