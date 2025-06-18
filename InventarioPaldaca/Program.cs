using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Utilidades;
using InventarioPaldaca.Utilidades.Filters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new VerificarSession());
});

builder.Services.AddDbContext<InventarioPaldacaContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventarioPaldacaContext"));
});

// Configuración de sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración
    options.Cookie.HttpOnly = true; // Seguridad adicional
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient<Dolar>();
builder.Services.AddHostedService<LimpiezaReportesService>();
builder.Services.AddScoped<FiltroActivosService>();
builder.Services.AddHttpContextAccessor(); // necesario para acceder a la sesión


var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Habilita el middleware de sesión
app.UseAuthorization();

// Aquí agregas la lógica de redirección dependiendo de la sesión
app.MapGet("/", context =>
{
    var session = context.Request.HttpContext.Session;
    var usuarioId = session.GetString("UsuarioId");

    if (string.IsNullOrEmpty(usuarioId))
    {
        // Redirigir al login si no hay sesión
        context.Response.Redirect("/acceso/login");
    }
    else
    {
        // Redirigir al Home si hay sesión
        context.Response.Redirect("/home/index");
    }
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();
