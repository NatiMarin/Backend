using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Respuesta
    {
        [Key]
        public int id_respuesta { get; set; }
        public string respuesta { get; set; }
        public int id_formulario { get; set; }
        public int id_pregunta { get; set; }
    }
}
