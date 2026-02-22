using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Santuario.Negocio.Interface;
using Santuario.Negocio.Interfaces;
using Santuario.Negocio.Negocios;
using Santuario.Negocio.Servicos;

namespace Santuario.Negocio.Extensions
{
    public static class ServiceCollectionExtensionsNegocio
    {
        public static IServiceCollection AddSantuarioNegocio(
            this IServiceCollection services,
            IConfiguration config,
            IHostEnvironment env)
        {
            // ============================
            // Negócio (centralizado)
            // ============================
            services.AddScoped<ILoginNegocio, LoginNegocio>();
            services.AddScoped<IUsuarioNegocio, UsuarioNegocio>();
            services.AddScoped<ICarrosselNegocio, CarrosselNegocio>();
            services.AddScoped<ISobreNegocio, SobreNegocio>();
            services.AddScoped<INoticiaNegocio, NoticiaNegocio>();

            return services;
        }
    }
}