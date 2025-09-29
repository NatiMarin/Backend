using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Historial_Medico
    {
        [Key]
        public int id_historialMedico { get; set; }
        public int id_animal {  get; set; }
        public DateTime fecha { get; set; }
        public string observacion { get; set; }
    }
}
