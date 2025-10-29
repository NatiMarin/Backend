using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Punto_Acopio
    {
        [Key]
        public int id_puntoAcopio { get; set; }

        public string nombre { get; set; } = string.Empty;

        public string calle { get; set; } = string.Empty;

        public int altura { get; set; }

        public string? departamento { get; set; }


        public int id_provincia { get; set; }

        public int id_localidad { get; set; }

        public string? descripcion { get; set; }

        public bool activo { get; set; } = true;
    }
}
