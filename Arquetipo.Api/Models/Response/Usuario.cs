using System.ComponentModel.DataAnnotations;

namespace Arquetipo.Api.Models.Response
{
    public class Usuario
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre de usuario utilizado para el inicio de sesión. Debe ser único.
        /// </summary>
        /// <example>juan.perez</example>
        [Required]
        [MaxLength(100)]
        public required string NombreUsuario { get; set; }

        /// <summary>
        /// Hash de la contraseña del usuario. Nunca almacenar la contraseña en texto plano.
        /// </summary>
        [Required]
        public required string PasswordHash { get; set; }

        /// <summary>
        /// Roles asignados al usuario, separados por comas (ej. "Admin,User") o en formato JSON.
        /// Puede ser nulo si el usuario no tiene roles específicos o si los roles se manejan en una tabla separada.
        /// </summary>
        /// <example>Admin,User</example>
        [MaxLength(200)]
        public string? Roles { get; set; }

        /// <summary>
        /// Indica si la cuenta de usuario está activa y puede iniciar sesión.
        /// Por defecto es true para nuevos usuarios.
        /// </summary>
        public bool EstaActivo { get; set; } = true;
    }
}
