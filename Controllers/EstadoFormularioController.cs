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
    public class EstadoFormularioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EstadoFormularioController(ApplicationDbContext context) => _context = context;

        // GET: api/estadoformulario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estado_Formulario>>> GetAll()
        {
            var data = await _context.Estado_Formulario
                .AsNoTracking()
                .OrderBy(e => e.id_estadoFormulario)
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/estadoformulario/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Estado_Formulario>> GetById(int id)
        {
            var item = await _context.Estado_Formulario
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.id_estadoFormulario == id);

            return item is null ? NotFound() : Ok(item);
        }

        // POST: api/estadoformulario
        [HttpPost]
        public async Task<ActionResult<Estado_Formulario>> Create([FromBody] Estado_Formulario dto)
        {
            dto.id_estadoFormulario = 0;

            if (string.IsNullOrWhiteSpace(dto.descripcion))
                return BadRequest("La descripción es obligatoria.");

            _context.Estado_Formulario.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_estadoFormulario }, dto);
        }

        // PUT: api/estadoformulario/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Estado_Formulario dto)
        {
            if (id != dto.id_estadoFormulario)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var exists = await _context.Estado_Formulario.AnyAsync(e => e.id_estadoFormulario == id);
            if (!exists) return NotFound();

            _context.Entry(dto).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/estadoformulario/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Estado_Formulario.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Estado_Formulario.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
