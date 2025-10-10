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

        // GET: /Usuario

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");

            // 🔹 Obtener usuarios
            var respUsuarios = await client.GetAsync("/api/usuario");
            if (!respUsuarios.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"Error al obtener usuarios ({(int)respUsuarios.StatusCode})";
                return View(Enumerable.Empty<Usuario>());
            }

            var jsonUsuarios = await respUsuarios.Content.ReadAsStringAsync();
            var usuarios = JsonSerializer.Deserialize<IEnumerable<Usuario>>(jsonUsuarios, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? Enumerable.Empty<Usuario>();

            // 🔹 Obtener estados de usuario
            var estados = new Dictionary<int, string>();
            var respEstados = await client.GetAsync("/api/Estado_Usuario");

            if (respEstados.IsSuccessStatusCode)
            {
                var jsonEstados = await respEstados.Content.ReadAsStringAsync();
                var listaEstados = JsonSerializer.Deserialize<IEnumerable<Estado_Usuario>>(jsonEstados, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (listaEstados != null)
                    estados = listaEstados.ToDictionary(e => e.id_estadoUsuario, e => e.descripcion);
            }
            else
            {
                ViewBag.ApiError = $"No se pudieron cargar los estados ({(int)respEstados.StatusCode})";
            }

            ViewBag.Estados = estados;

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            if (TempData["Error"] is string err) ViewBag.Error = err;

            return View(usuarios);
        }



        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var client = _http.CreateClient("Api");

                var respUsuario = await client.GetAsync($"/api/usuario/{id}");
                var bodyUsuario = await respUsuario.Content.ReadAsStringAsync();

                if (!respUsuario.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"No se pudo obtener el usuario {id}. Código {(int)respUsuario.StatusCode} {respUsuario.ReasonPhrase}. Respuesta: {bodyUsuario}";
                    return RedirectToAction(nameof(Index));
                }

                var usuario = JsonSerializer.Deserialize<Usuario>(bodyUsuario, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var respEstados = await client.GetAsync("/api/Estado_Usuario");

                Dictionary<int, string> estados = new();

                if (respEstados.IsSuccessStatusCode)
                {
                    var jsonEstados = await respEstados.Content.ReadAsStringAsync();
                    var listaEstados = JsonSerializer.Deserialize<IEnumerable<Estado_Usuario>>(jsonEstados, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (listaEstados != null)
                        estados = listaEstados.ToDictionary(e => e.id_estadoUsuario, e => e.descripcion);
                }

                ViewBag.Estados = estados;

                if (usuario == null)
                {
                    TempData["Error"] = $"El usuario {id} se recibió vacío. Respuesta: {bodyUsuario}";
                    return RedirectToAction(nameof(Index));
                }

                return View("Detalle", usuario);

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error interno: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Usuario/Crear

        // GET: /Usuario/Crear
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarEstadosAsync();
            var usuario = new Usuario
            {
                fechaAlta = DateTime.Now // 🔹 Fecha actual por defecto
            };
            return View(usuario);
        }

        // POST: /Usuario/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromForm] Usuario usuario)
        {
            // 🔹 Normalizar campos de texto
            usuario.nombre = FormatearTexto(usuario.nombre);
            usuario.apellido = FormatearTexto(usuario.apellido);
            usuario.direccion = FormatearTexto(usuario.direccion);
            usuario.departamento = FormatearTexto(usuario.departamento);
            usuario.email = usuario.email?.Trim();
            usuario.clave = usuario.clave?.Trim();

            // 🔹 Si la fecha no se envió, usar fecha actual
            if (usuario.fechaAlta == default)
                usuario.fechaAlta = DateTime.Now;

            if (!ModelState.IsValid)
            {
                await CargarEstadosAsync();
                return View(usuario);
            }

            try
            {
                var client = _http.CreateClient("Api");
                var content = new StringContent(JsonSerializer.Serialize(usuario), Encoding.UTF8, "application/json");
                var resp = await client.PostAsync("/api/usuario", content);

                var body = await resp.Content.ReadAsStringAsync();

                // 🔹 Mostrar detalle exacto si hay error
                if (!resp.IsSuccessStatusCode)
                {
                    ViewBag.ApiError = $"❌ Error al crear el usuario ({(int)resp.StatusCode} - {resp.ReasonPhrase})\n{body}";
                    await CargarEstadosAsync();
                    return View(usuario);
                }

                // 🔹 Todo ok
                TempData["Ok"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = $"⚠️ Error inesperado: {ex.Message}";
                await CargarEstadosAsync();
                return View(usuario);
            }
        }


        // 🔧 Helpers
        private string FormatearTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return texto;
            texto = texto.Trim().ToLower();
            return char.ToUpper(texto[0]) + texto.Substring(1);
        }

        private async Task CargarEstadosAsync()
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync("/api/Estado_Usuario");
            var estados = new Dictionary<int, string>();

            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<IEnumerable<Estado_Usuario>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (lista != null)
                    estados = lista.ToDictionary(e => e.id_estadoUsuario, e => e.descripcion);
            }

            ViewBag.Estados = estados;
        }


        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");

            // 🔹 Obtener usuario
            var respUsuario = await client.GetAsync($"/api/usuario/{id}");
            if (!respUsuario.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener el usuario.";
                return RedirectToAction(nameof(Index));
            }

            var jsonUsuario = await respUsuario.Content.ReadAsStringAsync();
            var usuario = JsonSerializer.Deserialize<Usuario>(jsonUsuario, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // 🔹 Obtener estados (IMPORTANTE)
            var estados = new Dictionary<int, string>();
            var respEstados = await client.GetAsync("/api/Estado_Usuario");

            if (respEstados.IsSuccessStatusCode)
            {
                var jsonEstados = await respEstados.Content.ReadAsStringAsync();
                var listaEstados = JsonSerializer.Deserialize<IEnumerable<Estado_Usuario>>(jsonEstados, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (listaEstados != null)
                    estados = listaEstados.ToDictionary(e => e.id_estadoUsuario, e => e.descripcion);
            }

            ViewBag.Estados = estados;

            return View(usuario);
        }



        // POST: /Usuario/Modificar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            var client = _http.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(usuario), Encoding.UTF8, "application/json");
            var resp = await client.PutAsync($"/api/usuario/{usuario.id_usuario}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT /api/usuario/{usuario.id_usuario} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(usuario);
            }

            TempData["Ok"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Usuario/Eliminar/5
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");

            // 🔹 Obtener usuario
            var respUsuario = await client.GetAsync($"/api/usuario/{id}");
            if (respUsuario.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "El usuario no existe o ya fue eliminado.";
                return RedirectToAction(nameof(Index));
            }

            if (!respUsuario.IsSuccessStatusCode)
            {
                var body = await respUsuario.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET /api/usuario/{id} -> {(int)respUsuario.StatusCode} {respUsuario.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var jsonUsuario = await respUsuario.Content.ReadAsStringAsync();
            var usuario = JsonSerializer.Deserialize<Usuario>(jsonUsuario, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // 🔹 Cargar estados (para que se muestre el nombre en la vista)
            var estados = new Dictionary<int, string>();
            var respEstados = await client.GetAsync("/api/Estado_Usuario");

            if (respEstados.IsSuccessStatusCode)
            {
                var jsonEstados = await respEstados.Content.ReadAsStringAsync();
                var listaEstados = JsonSerializer.Deserialize<IEnumerable<Estado_Usuario>>(jsonEstados, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (listaEstados != null)
                    estados = listaEstados.ToDictionary(e => e.id_estadoUsuario, e => e.descripcion);
            }

            ViewBag.Estados = estados;

            // 🔹 (Opcional) si querés mostrar el mensaje “no se puede eliminar porque está en uso”
            ViewBag.Bloqueado = false;
            ViewBag.Motivo = string.Empty;

            return View(usuario);
        }


        // POST: /Usuario/Eliminar/5
        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"/api/usuario/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict || resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = "No se puede eliminar el usuario porque está en uso.";
                if (!string.IsNullOrWhiteSpace(body)) TempData["ApiDetail"] = body;
                return RedirectToAction("Eliminar", new { id });
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"DELETE /api/usuario/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction("Eliminar", new { id });
            }

            TempData["Ok"] = "Usuario eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AgregarEstado([FromBody] Estado_Usuario estado)
        {
            if (estado == null || string.IsNullOrWhiteSpace(estado.descripcion))
                return BadRequest("La descripción es obligatoria.");

            var client = _http.CreateClient("Api");
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(estado),
                Encoding.UTF8,
                "application/json"
            );

            // Usa tu API real
            var resp = await client.PostAsync("/api/Estado_Usuario", content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, body);

            var creado = System.Text.Json.JsonSerializer.Deserialize<Estado_Usuario>(
                body,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return Json(new { id = creado.id_estadoUsuario, descripcion = creado.descripcion });
        }

    }

}
