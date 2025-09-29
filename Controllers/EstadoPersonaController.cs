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
    public class EstadoPersonaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EstadoPersonaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/estadopersona
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estado_Persona>>> GetAll()
        {
            var estados = await _context.Estado_Persona
                                        .AsNoTracking()
                                        .OrderBy(e => e.id_estadoPersona)
                                        .ToListAsync();
            return Ok(estados);
        }

        // GET: api/estadopersona/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Estado_Persona>> GetById(int id)
        {
            var estado = await _context.Estado_Persona
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(e => e.id_estadoPersona == id);
            return estado is null ? NotFound() : Ok(estado);
        }

        // POST: api/estadopersona
        [HttpPost]
        public async Task<ActionResult<Estado_Persona>> Create([FromBody] Estado_Persona dto)
        {
            dto.id_estadoPersona = 0;

            if (string.IsNullOrWhiteSpace(dto.descripcion))
                return BadRequest("La descripción es obligatoria.");

            _context.Estado_Persona.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_estadoPersona }, dto);
        }

        // PUT: api/estadopersona/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Estado_Persona dto)
        {
            if (id != dto.id_estadoPersona)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var exists = await _context.Estado_Persona.AnyAsync(e => e.id_estadoPersona == id);
            if (!exists) return NotFound();

            _context.Entry(dto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/estadopersona/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Estado_Persona.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Estado_Persona.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

