using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Estado
    {
        [Key]
        public int id_estado { get; set; }
        public string estado { get; set; }
    }
}
