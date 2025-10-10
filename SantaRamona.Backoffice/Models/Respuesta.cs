using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Backoffice.Models
{
    public class Respuesta
    {
        public int id_respuesta { get; set; }
        public int id_formulario { get; set; }
        public int id_pregunta { get; set; }

        [Required(ErrorMessage = "La respuesta es obligatoria")]
        public string respuesta { get; set; }
    }
}
