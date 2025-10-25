using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Provincia
    {
        [Key]
        public int id_provincia { get; set; }

        public string nombre { get; set; }

        public string codigo31662 { get; set; }


    }
}
