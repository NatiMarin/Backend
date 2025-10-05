using Microsoft.AspNetCore.Mvc;
using SantaRamona.Backoffice.Models;
using System.Text;
using System.Text.Json;

namespace SantaRamona.Backoffice.Controllers
{
    public class TamanoController : Controller
    {
        private readonly IHttpClientFactory _http;
        public TamanoController(IHttpClientFactory http) => _http = http;

        // GET: /Tamano
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync("/api/tamano");

            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"GET /api/tamano -> {(int)resp.StatusCode} {resp.ReasonPhrase}";
                return View(Enumerable.Empty<Tamano>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<IEnumerable<Tamano>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Enumerable.Empty<Tamano>();

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            if (TempData["Error"] is string err) ViewBag.Error = err;

            return View(data);
        }

        // GET: /Tamano/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Tamano());
        }

        // POST: /Tamano/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromForm] string tamano)
        {
            if (string.IsNullOrWhiteSpace(tamano))
            {
                ModelState.AddModelError(nameof(Tamano.tamano), "El tamaño es obligatoria.");
                return View(new Tamano { tamano = tamano ?? string.Empty });
            }

            var model = new Tamano { tamano = tamano.Trim() };
            var client = _http.CreateClient("Api");

            var content = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PostAsync("/api/tamano", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"POST /api/tamano -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model); // me quedo en pantalla con el error
            }

            // Éxito: me quedo en la misma pantalla para poder crear otro
            ViewBag.Ok = "Tamaño creado correctamente.";
            ModelState.Clear();
            return View(new Tamano());
        }

        // GET: /Tamano/Modificar/5
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"/api/tamano/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "El tamaño no existe.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET /api/tamano/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Tamano>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;

            return View(model);
        }

        // POST: /Tamano/Modificar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] int id_tamano, [FromForm] string tamano)
        {
            if (string.IsNullOrWhiteSpace(tamano))
            {
                ModelState.AddModelError(nameof(Tamano.tamano), "El tamaño es obligatoria.");
                return View(new Tamano { id_tamano = id_tamano, tamano = tamano ?? string.Empty });
            }

            var model = new Tamano { id_tamano = id_tamano, tamano = tamano.Trim() };
            var client = _http.CreateClient("Api");

            var content = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PutAsync($"/api/tamano/{id_tamano}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT /api/tamano/{id_tamano} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            TempData["Ok"] = "Tamaño actualizado correctamente.";
            return RedirectToAction(nameof(Modificar), new { id = id_tamano });
        }

        // GET: /Tamano/Eliminar/5
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"/api/tamano/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "El tamaño no existe o ya fue eliminado.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET /api/tamano/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Tamano>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return View(model); // Views/Tamano/Eliminar.cshtml
        }

        // POST: /Tamano/Eliminar/5
        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"/api/tamano/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict ||
                resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = "No se puede eliminar el tamaño porque está en uso.";
                if (!string.IsNullOrWhiteSpace(body)) TempData["ApiDetail"] = body;
                return RedirectToAction("Eliminar", new { id });
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"DELETE /api/tamano/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction("Eliminar", new { id });
            }

            TempData["Ok"] = "Tamaño eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
