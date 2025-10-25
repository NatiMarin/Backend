using System;
using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Formulario
    {
        [Key]
        public int id_formulario { get; set; }

        public int id_persona { get; set; }

        public int id_tipoFormulario { get; set; }

        public DateTime? fechaEnvio { get; set; }

        // Estado: Pendiente / Revisión / Aprobado / Denegado
        [StringLength(50)]
        public string? estado { get; set; } = "Pendiente";
    }
}
