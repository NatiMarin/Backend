using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoAnimalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EstadoAnimalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Estado
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estado_Animal>>> GetEstados()
        {
            return await _context.Estado_Animal.ToListAsync();
        }

        // GET: api/Estado/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Estado_Animal>> GetEstado(int id)
        {
            var estado = await _context.Estado_Animal.FindAsync(id);
            if (estado == null)
            {
                return NotFound();
            }
            return estado;
        }

        // POST: api/Estado
        [HttpPost]
        public async Task<ActionResult<Estado_Animal>> PostEstado(Estado_Animal estadoAnimal)
        {
            _context.Estado_Animal.Add(estadoAnimal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEstado), new { id = estadoAnimal.id_estadoAnimal }, estadoAnimal);
        }

        // PUT: api/Estado/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEstado(int id, Estado_Animal Estado_Animal)
        {
            if (id != Estado_Animal.id_estadoAnimal)
            {
                return BadRequest();
            }

            _context.Entry(Estado_Animal).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstadoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // DELETE: api/Estado/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstado(int id)
        {
            var estado = await _context.Estado_Animal.FindAsync(id);
            if (estado == null)
            {
                return NotFound();
            }

            _context.Estado_Animal.Remove(estado);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EstadoExists(int id)
        {
            return _context.Estado_Animal.Any(e => e.id_estadoAnimal == id);
        }
    }
}