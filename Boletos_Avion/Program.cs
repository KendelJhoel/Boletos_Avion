using Boletos_Avion.Controllers;
using Boletos_Avion.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Habilitar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Expira en 30 minutos
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Agregar servicios MVC
builder.Services.AddControllersWithViews();

// Aqui poner todos los services
builder.Services.AddScoped<DbController>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AccountController>();
builder.Services.AddScoped<VuelosService>();

builder.Services.AddScoped<AsientoService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<ReservaService>();
builder.Services.AddScoped<AsientoController>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<AeropuertoService>();



var app = builder.Build();

// Configuración del pipeline de la aplicación
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar sesiones antes de la autorización
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
