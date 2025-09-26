using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspecieController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EspecieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Especie
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Especie>>> GetEspecie()
        {
            return await _context.Especie.ToListAsync();
        }

        // GET: api/especie/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Especie>> GetEspecie(int id)
        {
            var especie = await _context.Especie.FindAsync(id);
            if (especie == null)
            {
                return NotFound();
            }
            return especie;
        }

        // POST: api/especie
        [HttpPost]
        public async Task<ActionResult<Especie>> PostEspecie(Especie especie)
        {
            _context.Especie.Add(especie);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEspecie), new { id = especie.id_especie }, especie);
        }

        // PUT: api/especie/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEspecie(int id, Especie especie)
        {
            if (id != especie.id_especie)
            {
                return BadRequest();
            }

            _context.Entry(especie).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EspecieExists(id))
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

        // DELETE: api/especie/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEspecie(int id)
        {
            var especie = await _context.Especie.FindAsync(id);
            if (especie == null)
            {
                return NotFound();
            }

            _context.Especie.Remove(especie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EspecieExists(int id)
        {
            return _context.Especie.Any(e => e.id_especie == id);
        }
    }
}