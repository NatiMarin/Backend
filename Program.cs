using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SantaRamona.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();

// Swagger/OpenAPI explícito
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SantaRamona",
        Version = "v1"
    });
});

// DbContext con guard de connection string (mensaje claro si falta)
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException(
        "Missing 'ConnectionStrings:DefaultConnection' in configuration. " +
        "Ensure appsettings.json in the site ROOT has it."
    );
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connStr));

var app = builder.Build();

// --- Middleware anti 500 mudo (déjalo arriba del pipeline) ---
app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "text/plain; charset=utf-8";
        await ctx.Response.WriteAsync("FATAL: " + ex.Message + "\n" + ex.StackTrace);
    }
});

// --- Swagger en Dev o cuando EnableSwagger=true ---
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", false);
if (app.Environment.IsDevelopment() || enableSwagger)
{
    // Genera JSON en /swagger/v1/swagger.json
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
        // c.SerializeAsV2 = false; // (opcional) fuerza OpenAPI 3.x
    });

    // UI en /swagger consumiendo endpoint relativo (evita issues de http/https o www)
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SantaRamona v1");
        c.RoutePrefix = "swagger";
    });
}

// Endpoints simples para sanity check
app.MapGet("/", () => Results.Ok(new { ok = true, root = true, time = DateTimeOffset.UtcNow }));
app.MapGet("/healthz", () => Results.Ok(new { ok = true, time = DateTimeOffset.UtcNow }));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
