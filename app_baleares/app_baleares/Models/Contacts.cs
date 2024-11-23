using NetCoreBackend.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace NetCoreBackend.Models
{
    public class Contacts
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Empresa { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Telefono { get; set; }
        [Required]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        public Address Direccion { get; set; }
    }
}
