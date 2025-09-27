using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using SantaRamona.Models;
using SantaRamona.Data;
using static System.Net.Mime.MediaTypeNames;

namespace SantaRamona.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistorialMedicoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public HistorialMedicoController(ApplicationDbContext context) => _context = context;

        // GET: api/historialmedico?pagina=1&pageSize=20&id_animal=5&desde=2025-01-01&hasta=2025-12-31
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Historial_Medico>>> GetAll(
            [FromQuery] int pagina = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? id_animal = null,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _context.Historial_Medico.AsNoTracking().AsQueryable();

            if (id_animal.HasValue) q = q.Where(h => h.id_animal == id_animal.Value);
            if (desde.HasValue) q = q.Where(h => h.fecha >= desde.Value);
            if (hasta.HasValue) q = q.Where(h => h.fecha <= hasta.Value);

            var data = await q.OrderByDescending(h => h.fecha)
                              .ThenByDescending(h => h.id_historialMedico)
                              .Skip((pagina - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();

            return Ok(data);
        }

        // GET: api/historialmedico/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Historial_Medico>> GetById(int id)
        {
            var item = await _context.Historial_Medico
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(h => h.id_historialMedico == id);
            return item is null ? NotFound() : Ok(item);
        }

        // POST: api/historialmedico
        [HttpPost]
        public async Task<ActionResult<Historial_Medico>> Create([FromBody] Historial_Medico dto)
        {
            // Identity: que lo genere SQL
            dto.id_historialMedico = 0;

            // Si no te mandan fecha, asigná ahora (tu tabla tiene DEFAULT GETDATE(), pero por las dudas)
            if (dto.fecha == default) dto.fecha = DateTime.Now;

            // Validación mínima
            if (dto.id_animal <= 0) return BadRequest("id_animal debe ser > 0.");
            if (string.IsNullOrWhiteSpace(dto.observacion)) return BadRequest("observacion es requerida.");

            _context.Historial_Medico.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_historialMedico }, dto);
        }

        // PUT: api/historialmedico/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Historial_Medico dto)
        {
            if (id != dto.id_historialMedico) return BadRequest("El id de la URL no coincide con el del animal.");

            var exists = await _context.Historial_Medico.AnyAsync(h => h.id_historialMedico == id);
            if (!exists) return NotFound();

            _context.Entry(dto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/historialmedico/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Historial_Medico.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Historial_Medico.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
