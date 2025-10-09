using Microsoft.AspNetCore.Mvc;
using SantaRamona.Backoffice.Models;
using System.Text;
using System.Text.Json;

namespace SantaRamona.Backoffice.Controllers
{
    public class EspecieController : Controller
    {
        private readonly IHttpClientFactory _http;
        public EspecieController(IHttpClientFactory http) => _http = http;

        // GET: /Especie
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync("/api/especie");

            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"GET /api/especie -> {(int)resp.StatusCode} {resp.ReasonPhrase}";
                return View(Enumerable.Empty<Especie>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<IEnumerable<Especie>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Enumerable.Empty<Especie>();

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            if (TempData["Error"] is string err) ViewBag.Error = err;

            return View(data);
        }

        // GET: /Especie/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Especie());
        }

        // POST: /Especie/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromForm] string especie)
        {
            if (string.IsNullOrWhiteSpace(especie))
            {
                ModelState.AddModelError(nameof(Especie.especie), "La especie es obligatoria.");
                return View(new Especie { especie = especie ?? string.Empty });
            }

            var model = new Especie { especie = especie.Trim() };
            var client = _http.CreateClient("Api");

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PostAsync("/api/especie", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"POST /api/especie -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model); // me quedo en pantalla con el error
            }

            // ✅ Éxito: me quedo en la misma pantalla para poder crear otra
            ViewBag.Ok = "Especie creada correctamente.";
            ModelState.Clear();                  // limpia validaciones
            return View(new Especie());          // deja el form vacío
        }

        /// GET: /Especie/Modificar/5
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"/api/especie/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "La especie no existe.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET /api/especie/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = System.Text.Json.JsonSerializer.Deserialize<Especie>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            // ✅ Mostrar mensaje de éxito si viene del POST
            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;

            return View(model);
        }

        // POST: /Especie/Modificar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] int id_especie, [FromForm] string especie)
        {
            if (string.IsNullOrWhiteSpace(especie))
            {
                ModelState.AddModelError(nameof(Especie.especie), "La especie es obligatoria.");
                return View(new Especie { id_especie = id_especie, especie = especie ?? string.Empty });
            }

            var model = new Especie { id_especie = id_especie, especie = especie.Trim() };
            var client = _http.CreateClient("Api");

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PutAsync($"/api/especie/{id_especie}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT /api/especie/{id_especie} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            // ✅ Igual que en Raza: seteo mensaje y redirijo al mismo Modificar (mismo ID)
            TempData["Ok"] = "Especie actualizada correctamente.";
            return RedirectToAction(nameof(Modificar), new { id = id_especie });
        }


        // GET: /Especie/Eliminar/5
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"/api/especie/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "La especie no existe o ya fue eliminada.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET /api/especie/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Especie>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(model);
        }

        // POST: /Especie/Eliminar/5
        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"/api/especie/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict || resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = "No se puede eliminar la especie porque está en uso.";
                if (!string.IsNullOrWhiteSpace(body)) TempData["ApiDetail"] = body;
                return RedirectToAction("Eliminar", new { id });
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"DELETE /api/especie/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction("Eliminar", new { id });
            }

            TempData["Ok"] = "Especie eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}