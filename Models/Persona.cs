﻿using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Persona
    {
        [Key]
        public int id_persona { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public int dni { get; set;}
        public DateTime? fechaNacimiento { get; set; }
        public string email { get; set; }
        public string telefono1 { get; set; }

        public string? telefono2 { get; set; }
        public string? calle { get; set; }
        public int? altura { get; set; }
        public string? departamento { get; set; }
        public int? id_localidad { get; set; }
        public int? id_provincia { get; set; }
        public string? redesSociales { get; set; }
        public DateTime fechaIngreso { get; set; }
        public DateTime? fechaEgreso { get; set; }
        public string? motivoEgreso { get; set; }
        public int? id_estadoPersona { get; set; }




    }
}
