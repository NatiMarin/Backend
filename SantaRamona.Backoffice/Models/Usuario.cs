using System;
using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Backoffice.Models
{
    public class Usuario
    {
        [Key]
        public int id_usuario { get; set; }

        [Required(ErrorMessage = "La clave es obligatoria.")]
        public string clave { get; set; }

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public string email { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        public string nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚ\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios.")]
        public string apellido { get; set; }

        public string direccion { get; set; }

        [Range(0, 99999, ErrorMessage = "Ingrese una altura válida (solo números).")]
        public int altura { get; set; }
        public string departamento { get; set; }

        [Range(1000000, 999999999, ErrorMessage = "Ingrese un teléfono válido.")]
        public int telefono1 { get; set; }

        [Range(1000000, 999999999, ErrorMessage = "Ingrese un teléfono válido.")]
        public int telefono2 { get; set; }

        public DateTime fechaAlta { get; set; }

        [Required(ErrorMessage = "Debe indicar la fecha de alta.")]
        public int id_estadoUsuario { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estado.")]
        public Estado_Usuario? Estado_Usuario { get; set; } // 🔹 navegación

       

    }
}
