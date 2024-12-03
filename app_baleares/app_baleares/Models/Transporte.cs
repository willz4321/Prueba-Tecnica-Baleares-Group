using NetCoreBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace app_baleares.Models
{
    public class Transporte
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Tipo { get; set; }
        [Required]
        public bool status { get; set; }

        [JsonIgnore]
        public ICollection<Contacts>? Contacts { get; set; }
    }
}


















