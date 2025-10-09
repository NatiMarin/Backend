using Microsoft.AspNetCore.Mvc;
using SantaRamona.Backoffice.Models;
using System.Text;
using System.Text.Json;

namespace SantaRamona.Backoffice.Controllers
{

    public class EstadoUsuarioController : Controller
    {
        private readonly IHttpClientFactory _http;
        public EstadoUsuarioController(IHttpClientFactory http) => _http = http;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync("/api/Estado_Usuario");

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"GET /api/Estado_Usuario -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(Enumerable.Empty<Estado_Usuario>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<IEnumerable<Estado_Usuario>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Enumerable.Empty<Estado_Usuario>();

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            if (TempData["Error"] is string err) ViewBag.Error = err;

            return View(data);
        }

        [HttpGet]
        public IActionResult Crear() => View(new Estado_Usuario());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromForm] string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                ModelState.AddModelError(nameof(Estado_Usuario.descripcion), "El estado es obligatorio.");
                return View(new Estado_Usuario { descripcion = descripcion ?? string.Empty });
            }

            var model = new Estado_Usuario { descripcion = descripcion.Trim() };
            var client = _http.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var resp = await client.PostAsync("/api/Estado_Usuario", content);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"POST /api/Estado_Usuario -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            ViewBag.Ok = "Estado creado correctamente.";
            ModelState.Clear();
            return View(new Estado_Usuario());
        }

        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"/api/Estado_Usuario/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "El estado no existe.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET /api/Estado_Usuario/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Estado_Usuario>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (model == null)
            {
                TempData["Error"] = "No se pudo deserializar el estado.";
                return RedirectToAction(nameof(Index));
            }

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            return View(model); // Vista Modificar.cshtml
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] Estado_Usuario model)
        {
            if (model == null || model.id_estadoUsuario <= 0)
            {
                ModelState.AddModelError("", "Identificador inválido.");
                return View(model ?? new Estado_Usuario());
            }

            if (string.IsNullOrWhiteSpace(model.descripcion))
            {
                ModelState.AddModelError(nameof(Estado_Usuario.descripcion), "El estado es obligatorio.");
                return View(model);
            }

            var client = _http.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var resp = await client.PutAsync($"/api/Estado_Usuario/{model.id_estadoUsuario}", content);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT /api/Estado_Usuario/{model.id_estadoUsuario} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            TempData["Ok"] = "Estado actualizado correctamente.";
            return RedirectToAction(nameof(Modificar), new { id = model.id_estadoUsuario });
        }


        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"/api/Estado_Usuario/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "El estado no existe o ya fue eliminado.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET /api/Estado_Usuario/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Estado_Usuario>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"/api/Estado_Usuario/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict ||
                resp.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                (int)resp.StatusCode == 422)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = "No se puede eliminar el estado porque está en uso.";
                if (!string.IsNullOrWhiteSpace(body)) TempData["ApiDetail"] = body;
                return RedirectToAction("Eliminar", new { id });
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"DELETE /api/Estado_Usuario/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction("Eliminar", new { id });
            }

            TempData["Ok"] = "Estado eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
