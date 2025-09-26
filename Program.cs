using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;

var builder = WebApplication.CreateBuilder(args);

// Añade los servicios para los controladores, crucial para las APIs.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inyecta el DbContext para que esté disponible en los controladores.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configura el pipeline de peticiones HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // Mapea los controladores
app.Run();