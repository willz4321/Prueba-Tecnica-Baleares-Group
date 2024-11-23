using System.ComponentModel.DataAnnotations;

namespace NetCoreBackend.Models
{
    public class Address
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Calle { get; set; }
        [Required]
        public string Localidad { get; set; }

    }
}
