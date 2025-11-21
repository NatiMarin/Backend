using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SantaRamona.Controllers
{
    // DTO para cambio de estado + usuario
    public class CambiarEstadoPersonaDto
    {
        public int id_estadoPersona { get; set; }   // nuevo estado
        public int id_usuario { get; set; }         // usuario que hizo el cambio
    }

    [ApiController]
    [Route("api/[controller]")]
    public class PersonaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PersonaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =======================
        // ⭐ Helper Capitalizar
        // =======================
        private string? Capitalizar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            texto = texto.Trim().ToLower();
            var partes = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < partes.Length; i++)
            {
                var p = partes[i];
                partes[i] = char.ToUpper(p[0]) + (p.Length > 1 ? p.Substring(1) : "");
            }

            return string.Join(' ', partes);
        }

        // ===================== GET (Paginado) =====================
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Persona>>> GetAll([FromQuery] int pagina = 1, [FromQuery] int pageSize = 20)
        //{
        //    if (pagina < 1) pagina = 1;
        //    if (pageSize < 1 || pageSize > 200) pageSize = 20;
        //
        //    var data = await _context.Persona
        //        .AsNoTracking()
        //        .OrderByDescending(p => p.id_persona)
        //       .Skip((pagina - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();
        //   return Ok(data);
        //}

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Persona>>> GetAll()
        {
            var data = await _context.Persona
                .AsNoTracking()
                .OrderBy(p => p.apellido)
                .ThenBy(p => p.nombre)
                .ToListAsync();

            return Ok(data);
        }


        // ===================== GET por ID =====================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Persona>> GetById(int id)
        {
            var persona = await _context.Persona
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.id_persona == id);

            return persona is null ? NotFound() : Ok(persona);
        }

        // ===================== POST =====================
        [HttpPost]
        public async Task<ActionResult<Persona>> Create([FromBody] Persona dto)
        {
            dto.id_persona = 0;

            // ⭐ Normalización / Capitalización
            dto.nombre = Capitalizar(dto.nombre);
            dto.apellido = Capitalizar(dto.apellido);
            dto.calle = Capitalizar(dto.calle);       
            dto.motivoEgreso = Capitalizar(dto.motivoEgreso);

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(dto.nombre))
                return BadRequest("El campo 'nombre' es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.apellido))
                return BadRequest("El campo 'apellido' es obligatorio.");

            if (dto.dni <= 0)
                return BadRequest("Debe ingresar un DNI válido (mayor a 0).");

            if (dto.fechaIngreso == default)
                dto.fechaIngreso = DateTime.Now;

            _context.Persona.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_persona }, dto);
        }

        // ===================== PUT COMPLETO =====================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Persona dto)
        {
            if (id != dto.id_persona)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            if (!await _context.Persona.AnyAsync(p => p.id_persona == id))
                return NotFound();

            // ⭐ Normalización / Capitalización
            dto.nombre = Capitalizar(dto.nombre);
            dto.apellido = Capitalizar(dto.apellido);
            dto.calle = Capitalizar(dto.calle);
            dto.departamento = Capitalizar(dto.departamento);
            dto.redesSociales = Capitalizar(dto.redesSociales);
            dto.motivoEgreso = Capitalizar(dto.motivoEgreso);

            _context.Entry(dto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Persona.AnyAsync(p => p.id_persona == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // ===================== PUT SOLO ESTADO + USUARIO =====================
        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoPersonaDto dto)
        {
            var persona = await _context.Persona.FindAsync(id);
            if (persona is null)
                return NotFound();

            persona.id_estadoPersona = dto.id_estadoPersona;
            persona.id_usuario = dto.id_usuario;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ===================== DELETE =====================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Persona.FindAsync(id);
            if (entity is null)
                return NotFound();

            _context.Persona.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
