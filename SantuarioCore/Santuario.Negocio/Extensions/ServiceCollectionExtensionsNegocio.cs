using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Santuario.Negocio.Interface;
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

            // Exemplo (quando você criar):
            // services.AddScoped<ICarrosselNegocio, CarrosselNegocio>();
            // services.AddScoped<ISobreNegocio, SobreNegocio>();
            // services.AddScoped<INoticiaNegocio, NoticiaNegocio>();
            // services.AddScoped<INoticiaComentarioNegocio, NoticiaComentarioNegocio>();

            return services;
        }
    }
}