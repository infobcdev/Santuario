using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Santuario.Entidade.Entities;
using Santuario.Negocio.Context;
using Santuario.Negocio.Helpers; 
using Santuario.Negocio.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Santuario.Negocio.Negocios
{
    public class SobreNegocio : ISobreNegocio
    {
        private readonly SantuarioDbContext _db;

        // padrão fixo do Sobre
        private const int TARGET_W = 1200;
        private const int TARGET_H = 800;

        private const string PASTA_PUBLICA = "Sobre";

        public SobreNegocio(SantuarioDbContext db)
        {
            _db = db;
        }

        public Task<Sobre?> BuscarPorIdAsync(int id)
            => _db.Set<Sobre>()
                .Include(x => x.Topicos)
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<List<Sobre>> ListarAsync(string pesquisar, bool? ativo)
        {
            var q = _db.Set<Sobre>()
                .AsNoTracking()
                .Include(x => x.Topicos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pesquisar))
            {
                var p = pesquisar.Trim();
                q = q.Where(x =>
                    x.Titulo1.Contains(p) ||
                    x.Descricao1.Contains(p) ||
                    (x.Titulo2 != null && x.Titulo2.Contains(p)) ||
                    (x.Descricao2 != null && x.Descricao2.Contains(p)));
            }

            if (ativo.HasValue)
                q = q.Where(x => x.Ativo == ativo.Value);

            return await q
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<(bool ok, string? erro)> SalvarAsync(
            int id,
            string? titulo1,
            string? descricao1,
            string? titulo2,
            string? descricao2,
            string? imagemUrlAtual,
            IFormFile? arquivoImagem,
            bool ativo,
            List<SobreTopico> topicos,
            int usuarioLogadoId,
            string webRootPath
        )
        {
            try
            {
                titulo1 = (titulo1 ?? "").Trim();
                descricao1 = (descricao1 ?? "").Trim();
                titulo2 = (titulo2 ?? "").Trim();
                descricao2 = (descricao2 ?? "").Trim();

                if (string.IsNullOrWhiteSpace(titulo1))
                    return (false, "Informe o Título 1.");

                if (string.IsNullOrWhiteSpace(descricao1))
                    return (false, "Informe a Descrição 1.");

                topicos ??= new List<SobreTopico>();

                // normaliza tópicos
                foreach (var t in topicos)
                {
                    if (t.Ordem <= 0) t.Ordem = 1;
                    t.Texto = (t.Texto ?? "").Trim();
                }

                // remove vazios
                topicos = topicos
                    .Where(t => !string.IsNullOrWhiteSpace(t.Texto))
                    .OrderBy(t => t.Ordem)
                    .ToList();

                // garante pasta física (wwwroot/Sobre)
                ImagemUploadHelper.EnsureFolder(webRootPath, PASTA_PUBLICA);

                if (id == 0)
                {
                    if (arquivoImagem == null || arquivoImagem.Length == 0)
                        return (false, "Selecione uma imagem (PNG ou JPG).");

                    // salva imagem
                    var rImg = await ImagemUploadHelper.SalvarOuSubstituirAsync(
                        arquivoImagem: arquivoImagem,
                        webRootPath: webRootPath,
                        pastaPublica: PASTA_PUBLICA,
                        tituloBase: titulo1,
                        targetW: TARGET_W,
                        targetH: TARGET_H,
                        currentImagemUrl: null
                    );

                    if (!rImg.ok)
                        return (false, rImg.erro);

                    var entity = new Sobre
                    {
                        ImagemUrl = rImg.imagemUrlNova!,
                        Titulo1 = titulo1,
                        Descricao1 = descricao1,
                        Titulo2 = string.IsNullOrWhiteSpace(titulo2) ? null : titulo2,
                        Descricao2 = string.IsNullOrWhiteSpace(descricao2) ? null : descricao2,
                        Ativo = ativo,
                        DataCriacao = DateTime.UtcNow,
                        IdUsuarioCriacao = usuarioLogadoId
                    };

                    _db.Add(entity);
                    await _db.SaveChangesAsync();

                    foreach (var t in topicos)
                    {
                        t.Id = 0;
                        t.IdSobre = entity.Id;
                        t.DataCriacao = DateTime.UtcNow;
                        t.IdUsuarioCriacao = usuarioLogadoId;
                        _db.Add(t);
                    }

                    await _db.SaveChangesAsync();
                    return (true, null);
                }
             
                var db = await _db.Set<Sobre>()
                    .Include(x => x.Topicos)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (db == null)
                    return (false, "Registro não encontrado.");

                // valida imagem no editar: só exige se não existir no banco
                if ((arquivoImagem == null || arquivoImagem.Length == 0) && string.IsNullOrWhiteSpace(db.ImagemUrl))
                    return (false, "Selecione uma imagem (PNG ou JPG).");

                db.Titulo1 = titulo1;
                db.Descricao1 = descricao1;
                db.Titulo2 = string.IsNullOrWhiteSpace(titulo2) ? null : titulo2;
                db.Descricao2 = string.IsNullOrWhiteSpace(descricao2) ? null : descricao2;
                db.Ativo = ativo;
                db.DataAlteracao = DateTime.UtcNow;
                db.IdUsuarioAlteracao = usuarioLogadoId;

                // troca imagem se veio arquivo novo
                if (arquivoImagem != null && arquivoImagem.Length > 0)
                {
                    var rNova = await ImagemUploadHelper.SalvarOuSubstituirAsync(
                        arquivoImagem: arquivoImagem,
                        webRootPath: webRootPath,
                        pastaPublica: PASTA_PUBLICA,
                        tituloBase: titulo1,
                        targetW: TARGET_W,
                        targetH: TARGET_H,
                        currentImagemUrl: db.ImagemUrl
                    );

                    if (!rNova.ok)
                        return (false, rNova.erro);

                    var caminhoFisicoAntigo = rNova.caminhoFisicoAntigo;

                    db.ImagemUrl = rNova.imagemUrlNova!;

                    await _db.SaveChangesAsync();

                    // apaga antiga depois de salvar
                    ImagemUploadHelper.TryDeleteFile(caminhoFisicoAntigo);
                }
                else
                {
                    await _db.SaveChangesAsync();
                }

                // atualiza tópicos: remove e recria
                if (db.Topicos != null && db.Topicos.Any())
                {
                    _db.RemoveRange(db.Topicos);
                    await _db.SaveChangesAsync();
                }

                foreach (var t in topicos)
                {
                    t.Id = 0;
                    t.IdSobre = db.Id;
                    t.DataCriacao = DateTime.UtcNow;
                    t.IdUsuarioCriacao = usuarioLogadoId;
                    _db.Add(t);
                }

                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch
            {
                return (false, "Não foi possível salvar o Sobre.");
            }
        }

        public async Task<(bool ok, string? erro)> InativarAsync(int id, int usuarioLogadoId)
        {
            try
            {
                var db = await _db.Set<Sobre>().FirstOrDefaultAsync(x => x.Id == id);
                if (db == null)
                    return (false, "Registro não encontrado.");

                db.Ativo = false;
                db.DataAlteracao = DateTime.UtcNow;
                db.IdUsuarioAlteracao = usuarioLogadoId;

                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch
            {
                return (false, "Não foi possível inativar.");
            }
        }
    }
}