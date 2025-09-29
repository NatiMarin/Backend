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
    public class PersonaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PersonaController(ApplicationDbContext context) => _context = context;

        // GET: api/persona?pagina=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Persona>>> GetAll([FromQuery] int pagina = 1, [FromQuery] int pageSize = 20)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var data = await _context.Persona
                .AsNoTracking()
                .OrderBy(p => p.id_persona)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/persona/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Persona>> GetById(int id)
        {
            var persona = await _context.Persona
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.id_persona == id);

            return persona is null ? NotFound() : Ok(persona);
        }
        [HttpPost]
        public async Task<ActionResult<Persona>> Create([FromBody] Persona dto)
        {
            dto.id_persona = 0;

            if (string.IsNullOrWhiteSpace(dto.nombre))
                return BadRequest("El campo 'nombre' es obligatorio.");

            if (dto.dni <= 0)
                return BadRequest("Debe ingresar un DNI válido (mayor a 0).");

            if (dto.fechaIngreso == default)
                dto.fechaIngreso = DateTime.Now;

            _context.Persona.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_persona }, dto);
        }

        // PUT: api/persona/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Persona dto)
        {
            if (id != dto.id_persona)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var exists = await _context.Persona.AnyAsync(p => p.id_persona == id);
            if (!exists) return NotFound();

            _context.Entry(dto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Persona.AnyAsync(p => p.id_persona == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/persona/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Persona.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Persona.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
