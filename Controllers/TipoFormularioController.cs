using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoFormularioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TipoFormularioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TipoFormulario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tipo_Formulario>>> GetTiposFormulario()
        {
            return await _context.Tipo_Formulario.ToListAsync();
        }

        // GET: api/TipoFormulario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tipo_Formulario>> GetTipoFormulario(int id)
        {
            var tipo = await _context.Tipo_Formulario.FindAsync(id);
            if (tipo == null)
            {
                return NotFound();
            }

            return tipo;
        }

        // POST: api/TipoFormulario
        [HttpPost]
        public async Task<ActionResult<Tipo_Formulario>> PostTipoFormulario(Tipo_Formulario tipo)
        {
            // Evita que EF mande NULL y anule el DEFAULT de SQL
            if (string.IsNullOrWhiteSpace(tipo.Estado))
                tipo.Estado = "Activo";

            _context.Tipo_Formulario.Add(tipo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTipoFormulario), new { id = tipo.id_tipoFormulario }, tipo);
        }

        // PUT: api/TipoFormulario/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoFormulario(int id, Tipo_Formulario tipo)
        {
            if (id != tipo.id_tipoFormulario)
            {
                return BadRequest();
            }

            // Normaliza estado (si llegara vacío)
            if (string.IsNullOrWhiteSpace(tipo.Estado))
                tipo.Estado = "Activo";

            _context.Entry(tipo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoFormularioExists(id))
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

        // DELETE: api/TipoFormulario/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoFormulario(int id)
        {
            var tipo = await _context.Tipo_Formulario.FindAsync(id);
            if (tipo == null)
            {
                return NotFound();
            }

            _context.Tipo_Formulario.Remove(tipo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TipoFormularioExists(int id)
        {
            return _context.Tipo_Formulario.Any(e => e.id_tipoFormulario == id);
        }
    }
}
