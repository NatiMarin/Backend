using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Backoffice.Models
{
    public class Pregunta
    {
        public int id_pregunta { get; set; }

        public int id_tipoFormulario { get; set; }

        [Required(ErrorMessage = "La pregunta es obligatoria")]
        public string pregunta { get; set; } = string.Empty;
    }
}
