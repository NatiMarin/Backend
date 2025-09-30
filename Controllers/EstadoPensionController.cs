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
    public class EstadoPensionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EstadoPensionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/estadopension
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estado_Pension>>> GetAll()
        {
            var data = await _context.Estado_Pension
                .AsNoTracking()
                .OrderBy(e => e.id_estadoPension)
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/estadopension/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Estado_Pension>> GetById(int id)
        {
            var item = await _context.Estado_Pension
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.id_estadoPension == id);

            return item is null ? NotFound() : Ok(item);
        }

        // POST: api/estadopension
        [HttpPost]
        public async Task<ActionResult<Estado_Pension>> Create([FromBody] Estado_Pension dto)
        {
            dto.id_estadoPension = 0;

            if (string.IsNullOrWhiteSpace(dto.descripcion))
                return BadRequest("La descripción es obligatoria.");

            _context.Estado_Pension.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_estadoPension }, dto);
        }

        // PUT: api/estadopension/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Estado_Pension dto)
        {
            if (id != dto.id_estadoPension)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var exists = await _context.Estado_Pension.AnyAsync(e => e.id_estadoPension == id);
            if (!exists) return NotFound();

            _context.Entry(dto).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/estadopension/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Estado_Pension.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Estado_Pension.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

