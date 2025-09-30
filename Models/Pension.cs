using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Pension
    {
        [Key]
        public int id_pension { get; set; }
        public string nombre { get; set; }
        public string email { get; set; }
        public string calle { get; set; }
        public string altura { get; set; }
        public string pisoCasa { get; set; }
        public string departamento { get; set; }
        public string barrio { get; set; }
        public int telefono { get; set; }
        public string redesSociales { get; set; }
        public DateTime fechaIngreso { get; set; }
        public DateTime fechaEgreso { get; set; }
        public int id_usuario { get; set; }
        public decimal montoDia { get; set; }
    }
}
