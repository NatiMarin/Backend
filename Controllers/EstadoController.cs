using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EstadoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Estado
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estado>>> GetEstados()
        {
            return await _context.Estado.ToListAsync();
        }

        // GET: api/Estado/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Estado>> GetEstado(int id)
        {
            var estado = await _context.Estado.FindAsync(id);
            if (estado == null)
            {
                return NotFound();
            }
            return estado;
        }

        // POST: api/Estado
        [HttpPost]
        public async Task<ActionResult<Estado>> PostEstado(Estado estado)
        {
            _context.Estado.Add(estado);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEstado), new { id = estado.id_estado }, estado);
        }

        // PUT: api/Estado/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEstado(int id, Estado Estado)
        {
            if (id != Estado.id_estado)
            {
                return BadRequest();
            }

            _context.Entry(Estado).State = EntityState.Modified;
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
            var estado = await _context.Estado.FindAsync(id);
            if (estado == null)
            {
                return NotFound();
            }

            _context.Estado.Remove(estado);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EstadoExists(int id)
        {
            return _context.Estado.Any(e => e.id_estado == id);
        }
    }
}