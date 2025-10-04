using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using SantaRamona.Backoffice.Models;

namespace SantaRamona.Backoffice.Controllers
{
    public class RazaController : Controller
    {
        private readonly IHttpClientFactory _http;
        public RazaController(IHttpClientFactory http) => _http = http;

        // GET: /Raza
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync("/api/raza");

            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"Error API: {(int)resp.StatusCode} - {resp.ReasonPhrase}";
                return View(new List<Raza>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var razas = JsonSerializer.Deserialize<List<Raza>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View("Index", razas ?? new List<Raza>());
        }

        // GET: /Raza/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            if (TempData["Ok"] is string ok)
                ViewBag.MensajeExito = ok;   // <- para mostrar el cartel en la vista

            return View(new SantaRamona.Backoffice.Models.Raza());
        }

        // POST: /Raza/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromForm] string raza)
        {
            Console.WriteLine($"[DEBUG] POST /Raza/Crear -> raza = '{raza}'");

            if (string.IsNullOrWhiteSpace(raza))
            {
                ModelState.AddModelError("raza", "La raza es obligatoria.");
                return View(new SantaRamona.Backoffice.Models.Raza { raza = raza });
            }

            var model = new SantaRamona.Backoffice.Models.Raza { raza = raza };

            var client = _http.CreateClient("Api");
            var json = System.Text.Json.JsonSerializer.Serialize(model);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var resp = await client.PostAsync("/api/raza", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"POST /api/raza -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            // ✅ Mostrar mensaje y quedarse en Crear para seguir cargando
            TempData["Ok"] = "Raza creada correctamente.";
            return RedirectToAction(nameof(Crear));
        }
        // GET: /Raza/Editar/{id}
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"/api/raza/{id}");

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"GET /api/raza/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Raza>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (TempData["Ok"] is string ok)
                ViewBag.MensajeExito = ok;

            return View(model);
        }

        // POST: /Raza/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] int id_raza, [FromForm] string raza)
        {
            if (string.IsNullOrWhiteSpace(raza))
            {
                ModelState.AddModelError("raza", "La raza es obligatoria.");
                return View(new Raza { id_raza = id_raza, raza = raza ?? string.Empty });
            }

            var model = new Raza { id_raza = id_raza, raza = raza };

            var client = _http.CreateClient("Api");
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await client.PutAsync($"/api/raza/{id_raza}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT /api/raza/{id_raza} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            TempData["Ok"] = "Raza actualizada correctamente.";
            return RedirectToAction(nameof(Modificar), new { id = id_raza });
        }
    }
}
