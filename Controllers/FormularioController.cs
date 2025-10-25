using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;

namespace SantaRamona.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormularioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FormularioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Formulario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Formulario>>> GetFormularios()
        {
            // Si preferís devolver solo ciertos campos, podés proyectar con .Select(...)
            return await _context.Formulario.AsNoTracking().ToListAsync();
        }

        // GET: api/Formulario/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Formulario>> GetFormulario(int id)
        {
            var f = await _context.Formulario.AsNoTracking().FirstOrDefaultAsync(x => x.id_formulario == id);
            if (f == null) return NotFound();
            return f;
        }

        // POST: api/Formulario
        [HttpPost]
        public async Task<ActionResult<Formulario>> PostFormulario(Formulario formulario)
        {
            // Normalizar estado si no vino
            if (string.IsNullOrWhiteSpace(formulario.estado))
                formulario.estado = "Pendiente";

            // No seteamos fechaEnvio: lo hace la DB (DEFAULT GETDATE())
            _context.Formulario.Add(formulario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFormulario), new { id = formulario.id_formulario }, formulario);
        }

        // PUT: api/Formulario/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutFormulario(int id, Formulario formulario)
        {
            if (id != formulario.id_formulario)
                return BadRequest();

            var f = await _context.Formulario.FirstOrDefaultAsync(x => x.id_formulario == id);
            if (f == null) return NotFound();

            // Actualizamos solo campos editables desde el backoffice
            f.id_persona = formulario.id_persona;
            f.id_tipoFormulario = formulario.id_tipoFormulario;
            f.estado = string.IsNullOrWhiteSpace(formulario.estado) ? f.estado : formulario.estado;
            // NO tocamos f.fechaEnvio (la conserva)

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FormularioExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Formulario/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFormulario(int id)
        {
            var f = await _context.Formulario.FindAsync(id);
            if (f == null) return NotFound();

            _context.Formulario.Remove(f);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool FormularioExists(int id)
            => _context.Formulario.Any(e => e.id_formulario == id);
    }
}
