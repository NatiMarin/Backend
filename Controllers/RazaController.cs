using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data; 

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RazaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RazaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Raza
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Raza>>> GetRazas()
        {
            return await _context.Raza.ToListAsync();
        }

        // GET: api/Raza/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Raza>> GetRaza(int id)
        {
            var raza = await _context.Raza.FindAsync(id);
            if (raza == null)
            {
                return NotFound();
            }
            return raza;
        }

        // POST: api/Raza
        [HttpPost]
        public async Task<ActionResult<Raza>> PostRaza(Raza raza)
        {
            _context.Raza.Add(raza);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRaza), new { id = raza.id_raza }, raza);
        }

        // PUT: api/Raza/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRaza(int id, Raza raza)
        {
            if (id != raza.id_raza)
            {
                return BadRequest();
            }

            _context.Entry(raza).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RazaExists(id))
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

        // DELETE: api/Raza/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRaza(int id)
        {
            var raza = await _context.Raza.FindAsync(id);
            if (raza == null)
            {
                return NotFound();
            }

            _context.Raza.Remove(raza);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RazaExists(int id)
        {
            return _context.Raza.Any(e => e.id_raza == id);
        }
    }
}