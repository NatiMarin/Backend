using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DonacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Donacion
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Donacion>>> GetDonaciones()
        {
            return await _context.Donacion.ToListAsync();
        }

        // GET: api/Donacion/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Donacion>> GetDonacion(int id)
        {
            var donacion = await _context.Donacion.FindAsync(id);
            if (donacion == null)
            {
                return NotFound();
            }

            return donacion;
        }

        // POST: api/Donacion
        [HttpPost]
        public async Task<ActionResult<Donacion>> PostDonacion(Donacion donacion)
        {
            _context.Donacion.Add(donacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDonacion), new { id = donacion.id_donacion }, donacion);
        }

        // PUT: api/Donacion/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDonacion(int id, Donacion donacion)
        {
            if (id != donacion.id_donacion)
            {
                return BadRequest();
            }

            _context.Entry(donacion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DonacionExists(id))
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

        // DELETE: api/Donacion/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonacion(int id)
        {
            var donacion = await _context.Donacion.FindAsync(id);
            if (donacion == null)
            {
                return NotFound();
            }

            _context.Donacion.Remove(donacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DonacionExists(int id)
        {
            return _context.Donacion.Any(e => e.id_donacion == id);
        }
    }
}
