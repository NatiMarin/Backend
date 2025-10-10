using Microsoft.AspNetCore.Mvc;
using SantaRamona.Backoffice.Models;
using System.Text;
using System.Text.Json;

namespace SantaRamona.Backoffice.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IHttpClientFactory _http;
        public UsuarioController(IHttpClientFactory http) => _http = http;

        // =================== INDEX ===================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var respUsuarios = await client.GetAsync("/api/usuario");

            if (!respUsuarios.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"Error al obtener usuarios ({(int)respUsuarios.StatusCode})";
                return View(Enumerable.Empty<Usuario>());
            }

            var jsonUsuarios = await respUsuarios.Content.ReadAsStringAsync();
            var usuarios = JsonSerializer.Deserialize<IEnumerable<Usuario>>(jsonUsuarios,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Enumerable.Empty<Usuario>();

            await CargarEstados();

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            if (TempData["Error"] is string err) ViewBag.Error = err;

            return View(usuarios);
        }

        // =================== CREAR ===================
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarEstados();
            return View(new Usuario());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario usuario)
        {
            try
            {
                // 🚨 SOLO PARA DEBUG
                Console.WriteLine($"Recibido: {usuario.nombre}, {usuario.email}, {usuario.id_estadoUsuario}");

                var client = _http.CreateClient("Api");
                var content = new StringContent(JsonSerializer.Serialize(usuario), Encoding.UTF8, "application/json");
                var resp = await client.PostAsync("/api/usuario", content);

                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    ViewBag.ApiError = "Error al cargar el usuario. Revise los campos antes de guardar.";
                    // 🔹 Recargo estados para mantener combo
                    await CargarEstados();
                    return View(usuario);
                }

                TempData["Ok"] = "✅ Usuario creado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = $"Excepción: {ex.Message}";
                await CargarEstados();
                return View(usuario);
            }
        }


        // =================== DETALLE ===================
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"/api/usuario/{id}");

            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo obtener el usuario.";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var usuario = JsonSerializer.Deserialize<Usuario>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            await CargarEstados();
            return View(usuario);
        }

        // =================== MODIFICAR ===================
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            try
            {
                var client = _http.CreateClient("Api");
                var resp = await client.GetAsync($"/api/usuario/{id}");

                if (!resp.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Error al cargar el usuario.";
                    return RedirectToAction(nameof(Index));
                }

                var json = await resp.Content.ReadAsStringAsync();
                var usuario = JsonSerializer.Deserialize<Usuario>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (usuario == null)
                {
                    TempData["Error"] = "Error al cargar el usuario.";
                    return RedirectToAction(nameof(Index));
                }

                await CargarEstados();
                return View(usuario);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al cargar el usuario.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] Usuario usuario)
        {
            try
            {
                var client = _http.CreateClient("Api");
                var content = new StringContent(JsonSerializer.Serialize(usuario), Encoding.UTF8, "application/json");
                var resp = await client.PutAsync($"/api/usuario/{usuario.id_usuario}", content);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    ViewBag.ApiError = "Error al modificar el usuario. Revise los campos antes de guardar.";
                    await CargarEstados();
                    return View(usuario);
                }

                TempData["Ok"] = "✅ Usuario modificado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.ApiError = "Error al modificar el usuario. Revise los campos antes de guardar.";
                await CargarEstados();
                return View(usuario);
            }
        }

        // =================== ELIMINAR ===================
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"/api/usuario/{id}");

            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "El usuario no existe o ya fue eliminado.";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Usuario>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            await CargarEstados();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"/api/usuario/{id}");

            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"Error al eliminar usuario ({(int)resp.StatusCode})";
                return RedirectToAction(nameof(Index));
            }

            TempData["Ok"] = "Usuario eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // =================== AGREGAR ESTADO ===================
        [HttpPost]
        public async Task<IActionResult> AgregarEstado([FromBody] Estado_Usuario estado)
        {
            if (estado == null || string.IsNullOrWhiteSpace(estado.descripcion))
                return BadRequest("La descripción es obligatoria.");

            var client = _http.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(estado), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/Estado_Usuario", content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, body);

            var creado = JsonSerializer.Deserialize<Estado_Usuario>(
                body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Json(new { id = creado.id_estadoUsuario, descripcion = creado.descripcion });
        }

        // =================== HELPERS ===================
        private async Task CargarEstados()
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync("/api/Estado_Usuario");
            var estados = new Dictionary<int, string>();

            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<IEnumerable<Estado_Usuario>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (lista != null)
                    estados = lista.ToDictionary(e => e.id_estadoUsuario, e => e.descripcion);
            }
            ViewBag.Estados = estados;
        }
    }
}
