using Microsoft.EntityFrameworkCore;
using Santuario.Entidade.Entities;
using Santuario.Negocio.Context;
using Santuario.Negocio.Helpers.Security;
using Santuario.Negocio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Santuario.Negocio.Servicos
{
    public class UsuarioNegocio : IUsuarioNegocio
    {
        private readonly SantuarioDbContext _db;

        public UsuarioNegocio(SantuarioDbContext db)
        {
            _db = db;
        }

        public async Task<List<Usuario>> ListarAsync(string? pesquisar, bool? ativo, TipoUsuario? tipo)
        {
            var q = _db.Usuarios.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(pesquisar))
            {
                var p = pesquisar.Trim();
                q = q.Where(x => x.Nome.Contains(p) || x.Login.Contains(p));
            }

            if (ativo.HasValue)
                q = q.Where(x => x.Ativo == ativo.Value);

            if (tipo.HasValue)
                q = q.Where(x => x.Tipo == tipo.Value);

            return await q
                .OrderBy(x => x.Nome)
                .ToListAsync();
        }

        public Task<Usuario?> BuscarPorIdAsync(int id)
            => _db.Usuarios.FirstOrDefaultAsync(x => x.Id == id);

        public Task<Usuario?> BuscarPorLoginAsync(string login, int? ignorarId = null)
        {
            var l = login.Trim();
            var q = _db.Usuarios.AsQueryable().Where(x => x.Login == l);
            if (ignorarId.HasValue)
                q = q.Where(x => x.Id != ignorarId.Value);

            return q.FirstOrDefaultAsync();
        }

        public async Task<(bool ok, string? erro)> SalvarAsync(
            int id,
            string nome,
            string login,
            string? senha,
            TipoUsuario tipo,
            bool ativo,
            int usuarioLogadoId
        )
        {
            nome = (nome ?? "").Trim();
            login = (login ?? "").Trim();

            if (string.IsNullOrWhiteSpace(nome))
                return (false, "Informe o nome.");

            if (string.IsNullOrWhiteSpace(login))
                return (false, "Informe o login.");

            var existeLogin = await BuscarPorLoginAsync(login, ignorarId: id == 0 ? null : id);
            if (existeLogin != null)
                return (false, "Já existe um usuário com este login.");

            Usuario entity;

            if (id == 0)
            {
                entity = new Usuario
                {
                    Nome = nome,
                    Login = login,
                    Tipo = tipo,
                    Ativo = ativo
                };

                if (string.IsNullOrWhiteSpace(senha))
                    return (false, "Informe a senha para criar o usuário.");

                (entity.SenhaHash, entity.SenhaSalt) = SenhaHelper.CriarSenha(senha);

                // auditoria (ajuste conforme seu EntidadeAuditoria)
                entity.DataCriacao = DateTime.UtcNow;
                entity.IdUsuarioCriacao = usuarioLogadoId;

                _db.Usuarios.Add(entity);
            }
            else
            {
                entity = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    return (false, "Usuário não encontrado.");

                entity.Nome = nome;
                entity.Login = login;
                entity.Tipo = tipo;
                entity.Ativo = ativo;

                if (!string.IsNullOrWhiteSpace(senha))
                {
                    (entity.SenhaHash, entity.SenhaSalt) = SenhaHelper.CriarSenha(senha);
                }

                entity.DataAlteracao = DateTime.UtcNow;
                entity.IdUsuarioAlteracao = usuarioLogadoId;
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? erro)> InativarAsync(int id, int usuarioLogadoId)
        {
            var entity = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return (false, "Usuário não encontrado.");

            entity.Ativo = false;
            entity.DataAlteracao = DateTime.UtcNow;
            entity.IdUsuarioAlteracao = usuarioLogadoId;

            await _db.SaveChangesAsync();
            return (true, null);
        }

    
    }
}