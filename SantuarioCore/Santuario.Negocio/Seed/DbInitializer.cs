using Microsoft.EntityFrameworkCore;
using Santuario.Entidade.Entities;
using Santuario.Negocio.Context;
using Santuario.Negocio.Helpers.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Santuario.Negocio.Seed
{
    public static class DbInitializer
    {
        public static async Task InicializarAsync(SantuarioDbContext context)
        {
            await context.Database.MigrateAsync();

            // Verifica se já existe usuário
            if (await context.Set<Usuario>().AnyAsync())
                return;

            var (hash, salt) = SenhaHelper.CriarSenha("admin123");

            var admin = new Usuario
            {
                Nome = "Administrador",
                Login = "admin",
                SenhaHash = hash,
                SenhaSalt = salt,
                Ativo = true,
                Tipo = TipoUsuario.Administrador,
                DataCriacao = DateTime.UtcNow
            };

            context.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}