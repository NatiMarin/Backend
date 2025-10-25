using System.ComponentModel.DataAnnotations.Schema;

namespace SantaRamona.Models
{
    [Table("ROL_PERMISO")]
    public class Rol_Permiso
    {
        [Column("id_rol")]
        public int id_rol { get; set; }

        [Column("id_permiso")]
        public int id_permiso { get; set; }

        [ForeignKey("id_rol")]
        public Rol Rol { get; set; }

        [ForeignKey("id_permiso")]
        public Permiso Permiso { get; set; }
    }
}
