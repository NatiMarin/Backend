using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SantaRamona.Models
{
    public class Localidad
    {
        [Key]
        public int id_localidad { get; set; }
        public int id_provincia { get; set; }
        public string nombre { get; set; }
        public int codigopostal { get; set; }

    }
}
