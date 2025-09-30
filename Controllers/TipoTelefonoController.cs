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
    public class TipoTelefonoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TipoTelefonoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/tipotelefono
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tipo_Telefono>>> GetAll()
        {
            var data = await _context.Tipo_Telefono
                .AsNoTracking()
                .OrderBy(t => t.id_tipoTelefono)
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/tipotelefono/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tipo_Telefono>> GetById(int id)
        {
            var item = await _context.Tipo_Telefono
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.id_tipoTelefono == id);

            return item is null ? NotFound() : Ok(item);
        }

        // POST: api/tipotelefono
        [HttpPost]
        public async Task<ActionResult<Tipo_Telefono>> Create([FromBody] Tipo_Telefono dto)
        {
            dto.id_tipoTelefono = 0;

            if (string.IsNullOrWhiteSpace(dto.descripcion))
                return BadRequest("La descripción es obligatoria.");

            _context.Tipo_Telefono.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_tipoTelefono }, dto);
        }

        // PUT: api/tipotelefono/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Tipo_Telefono dto)
        {
            if (id != dto.id_tipoTelefono)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var exists = await _context.Tipo_Telefono.AnyAsync(t => t.id_tipoTelefono == id);
            if (!exists) return NotFound();

            _context.Entry(dto).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/tipotelefono/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Tipo_Telefono.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Tipo_Telefono.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
