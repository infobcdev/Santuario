using Microsoft.EntityFrameworkCore;
using Santuario.Entidade.Entities;
using Santuario.Negocio.Context;
using Santuario.Negocio.Helpers.Security;
using Santuario.Negocio.Interface;
using System;
using System.Threading.Tasks;

namespace Santuario.Negocio.Servicos
{
    public class LoginNegocio : ILoginNegocio
    {
        private readonly SantuarioDbContext _db;

        public LoginNegocio(SantuarioDbContext db)
        {
            _db = db;
        }

        public async Task<Usuario?> AutenticarAsync(string login, string senha)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
                return null;

            // Ajuste para evitar erro por espaços
            login = login.Trim();

            var usuario = await _db.Set<Usuario>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Login == login);

            if (usuario == null)
                return null;

            if (!usuario.Ativo)
                return null;

            var ok = SenhaHelper.VerificarSenha(
                senhaDigitada: senha,
                hashArmazenado: usuario.SenhaHash,
                saltArmazenado: usuario.SenhaSalt
            );

            return ok ? usuario : null;
        }
    }
}