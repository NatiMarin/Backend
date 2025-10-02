using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SantaRamona.Data;
using SantaRamona.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace SantaRamona.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _cfg;

        public AuthController(ApplicationDbContext context, IConfiguration cfg)
        {
            _context = context;
            _cfg = cfg;
        }

        // POST: /api/auth/register
        // Crea usuario con clave hasheada. Úsalo para bootstrap/panel interno.
        [HttpPost("register")]
        public async Task<ActionResult<Usuario>> Register([FromBody] Usuario dto)
        {
            if (string.IsNullOrWhiteSpace(dto.email) || string.IsNullOrWhiteSpace(dto.clave))
                return BadRequest("Email y clave son obligatorios.");

            var exists = await _context.Usuario.AnyAsync(u => u.email == dto.email);
            if (exists) return Conflict("Ya existe un usuario con ese email.");

            // Hash de contraseña
            dto.clave = BC.HashPassword(dto.clave);
            if (dto.fechaAlta == default) dto.fechaAlta = DateTime.UtcNow;

            _context.Usuario.Add(dto);
            await _context.SaveChangesAsync();

            // No devolvemos la clave
            dto.clave = "***";
            return Ok(dto);
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthModel.LoginResponse>> Login([FromBody] AuthModel.LoginRequest req)
        {
            // Proyectamos sólo lo necesario para evitar desajustes de tipos
            var user = await _context.Usuario
                .AsNoTracking()
                .Where(u => u.email == req.Username)
                .Select(u => new { u.id_usuario, u.email, u.clave })
                .SingleOrDefaultAsync();

            if (user is null) return Unauthorized("Usuario o clave inválidos.");

            // Verificación con BCrypt (DB guarda HASH)
            if (!BC.Verify(req.Password, user.clave))
                return Unauthorized("Usuario o clave inválidos.");

            // Claims mínimos
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.id_usuario.ToString()),
                new(ClaimTypes.Name, user.email),
                new(ClaimTypes.Email, user.email),
                new("scope", "backend")
            };

            // Generar JWT
            var rawKey = _cfg["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(rawKey) || Encoding.UTF8.GetByteCount(rawKey) < 32)
                return Problem("Jwt:Key debe tener >= 32 bytes (256 bits) para HS256.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(int.TryParse(_cfg["Jwt:ExpiresMinutes"], out var m) ? m : 60);

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new AuthModel.LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAtUtc = expires
            };
        }       
    }
}
