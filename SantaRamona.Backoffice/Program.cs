using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// --- Servicios ---
builder.Services.AddHttpClient("Api", client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Falta configurar Api:BaseUrl en appsettings.json");

    client.BaseAddress = new Uri(baseUrl!);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- Middleware ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// app.UseAuthorization();

// Ruta por defecto 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
