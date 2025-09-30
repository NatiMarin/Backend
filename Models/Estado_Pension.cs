using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Estado_Pension
    {
        [Key]
        public int id_estadoPension { get; set; }
        public string descripcion { get; set; }
    }
}
