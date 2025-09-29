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
    public class PensionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PensionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/pension?pagina=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pension>>> GetAll([FromQuery] int pagina = 1, [FromQuery] int pageSize = 20)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var data = await _context.Pension
                .AsNoTracking()
                .OrderBy(p => p.id_pension)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/pension/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Pension>> GetById(int id)
        {
            var pension = await _context.Pension.AsNoTracking().FirstOrDefaultAsync(p => p.id_pension == id);
            return pension is null ? NotFound() : Ok(pension);
        }

        // POST: api/pension
        [HttpPost]
        public async Task<ActionResult<Pension>> Create([FromBody] Pension dto)
        {
            // Identity generado por SQL
            dto.id_pension = 0;

            // Validaciones mínimas
            if (string.IsNullOrWhiteSpace(dto.nombre))
                return BadRequest("El nombre es requerido.");
            if (dto.id_usuario <= 0)
                return BadRequest("id_usuario debe ser > 0.");
            if (dto.montoDia <= 0)
                return BadRequest("montoDia debe ser > 0.");

            _context.Pension.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_pension }, dto);
        }

        // PUT: api/pension/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Pension dto)
        {
            if (id != dto.id_pension)
                return BadRequest("El id de la URL no coincide con el del cuerpo.");

            var exists = await _context.Pension.AnyAsync(p => p.id_pension == id);
            if (!exists) return NotFound();

            _context.Entry(dto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Pension.AnyAsync(p => p.id_pension == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/pension/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Pension.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Pension.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
