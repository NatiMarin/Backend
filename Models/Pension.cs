using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SantaRamona.Models
{
    public class Pension
    {
        [Key]
        public int id_pension { get; set; }
        public string? nombre { get; set; } = string.Empty;
        public string? email { get; set; } = string.Empty;
        public string telefono1 { get; set; } = string.Empty; 
        public string? telefono2 { get; set; }
        public string calle { get; set; }
        public int altura { get; set; }       
        public string? departamento { get; set; }
        public int id_provincia { get; set; }
        public int id_localidad { get; set; }                
        public string? redesSociales { get; set; }
        public int id_estadoPension { get; set; }
        public int id_usuario { get; set; }
        public DateTime fechaIngreso { get; set; } = DateTime.Now;

        public DateTime? fechaEgreso { get; set; }
        public decimal? montoDia { get; set; }
    }
}
