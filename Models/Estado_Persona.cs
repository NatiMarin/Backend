using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Estado_Persona
    {
        [Key]
        public int id_estadoPersona { get; set; }
        public string descripcion { get; set; }
    }
}
