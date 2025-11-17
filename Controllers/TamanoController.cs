using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TamanoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TamanoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================
        // Helper para normalizar nombre de tamaño
        // ========================
        private string? NormalizarTamano(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            texto = texto.Trim().ToLower();

            // Primera letra en mayúscula, resto minúscula
            return char.ToUpper(texto[0]) + (texto.Length > 1 ? texto.Substring(1) : "");
        }

        // GET: api/Tamano
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tamano>>> GetTamanos()
        {
            return await _context.Tamano.ToListAsync();
        }

        // GET: api/Tamano/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tamano>> GetTamano(int id)
        {
            var tamano = await _context.Tamano.FindAsync(id);
            if (tamano == null)
            {
                return NotFound();
            }
            return tamano;
        }

        // POST: api/Tamano
        [HttpPost]
        public async Task<ActionResult<Tamano>> PostTamano(Tamano tamano)
        {
            // Normalizar nombre
            tamano.tamano = NormalizarTamano(tamano.tamano);

            _context.Tamano.Add(tamano);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTamano), new { id = tamano.id_tamano }, tamano);
        }

        // PUT: api/Tamano/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTamano(int id, Tamano tamano)
        {
            if (id != tamano.id_tamano)
            {
                return BadRequest();
            }

            // Normalizar nombre en modificación
            tamano.tamano = NormalizarTamano(tamano.tamano);

            _context.Entry(tamano).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TamanoExists(id))
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

        // DELETE: api/Tamano/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTamano(int id)
        {
            var tamano = await _context.Tamano.FindAsync(id);
            if (tamano == null)
            {
                return NotFound();
            }

            _context.Tamano.Remove(tamano);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TamanoExists(int id)
        {
            return _context.Tamano.Any(e => e.id_tamano == id);
        }
    }
}
