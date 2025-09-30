using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Seguimiento
    {
        [Key]
        public int id_seguimiento { get; set; }
        public int id_animal { get; set; }
        public DateTime fecha { get; set; }
        public string descripcion {  get; set; }
    }
}
