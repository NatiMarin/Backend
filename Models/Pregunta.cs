using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Pregunta
    {
        [Key]
        public int id_pregunta { get; set; }

        public int id_tipoFormulario { get; set; }

        public string pregunta { get; set; }

        public int? orden { get; set; }
    }
}
