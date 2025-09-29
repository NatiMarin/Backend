using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantaRamona.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeguimientoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeguimientoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/seguimiento?pagina=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seguimiento>>> GetAll([FromQuery] int pagina = 1, [FromQuery] int pageSize = 20)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var data = await _context.Seguimiento
                .AsNoTracking()
                .OrderBy(s => s.id_seguimiento)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/seguimiento/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Seguimiento>> GetById(int id)
        {
            var seguimiento = await _context.Seguimiento
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.id_seguimiento == id);

            return seguimiento is null ? NotFound() : Ok(seguimiento);
        }

        // POST: api/seguimiento
        [HttpPost]
        public async Task<ActionResult<Seguimiento>> Create([FromBody] Seguimiento dto)
        {
            // Identity generado por SQL
            dto.id_seguimiento = 0;

            // Validaciones mínimas
            if (dto.id_animal <= 0)
                return BadRequest("id_animal debe ser > 0.");
            if (string.IsNullOrWhiteSpace(dto.descripcion))
                return BadRequest("descripcion es requerida.");

            // Si no mandan fecha, usamos la actual
            if (dto.fecha == default)
                dto.fecha = DateTime.Now;

            _context.Seguimiento.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_seguimiento }, dto);
        }

        // PUT: api/seguimiento/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Seguimiento dto)
        {
            if (id != dto.id_seguimiento)
                return BadRequest("El id de la URL no coincide con el del cuerpo.");

            var exists = await _context.Seguimiento.AnyAsync(s => s.id_seguimiento == id);
            if (!exists) return NotFound();

            _context.Entry(dto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Seguimiento.AnyAsync(s => s.id_seguimiento == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/seguimiento/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Seguimiento.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Seguimiento.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
