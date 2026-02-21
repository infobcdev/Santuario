using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Santuario.Entidade.Helpers;
using Santuario.Entidade.Middleware;
using Santuario.Negocio.Context;
using Santuario.Negocio.Extensions;
using Santuario.Negocio.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SantuarioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Log em arquivo (DI)
builder.Services.AddSingleton<LogManager>();

// Negócio centralizado
builder.Services.AddSantuarioNegocio(builder.Configuration, builder.Environment);

// Auth
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Login";
        opt.LogoutPath = "/Login/Sair";
        opt.AccessDeniedPath = "/Error/403";
        opt.ExpireTimeSpan = TimeSpan.FromDays(7);
        opt.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SantuarioDbContext>();
    await DbInitializer.InicializarAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// ✅ 1) Middleware de exceções (antes de routing)
app.UseSantuarioExceptionHandling();

// ✅ 2) StatusCode pages (404/403 etc sem exception)
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();