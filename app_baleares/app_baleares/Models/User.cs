using NetCoreBackend.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace NetCoreBackend.Models
{
    public class User
    {
        [Required]
        public int Id { get; set; }
        public string Nombre { get; set; }
        [Required]
        [EmailAddress]
        public string Correo { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public Rol Rol { get; set; }
        public string Avatar { get; set; }
    }
}
