using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Tipo_Telefono
    {
        [Key]
        public int id_tipoTelefono { get; set; }
        public string descripcion { get; set; }
    }
}
