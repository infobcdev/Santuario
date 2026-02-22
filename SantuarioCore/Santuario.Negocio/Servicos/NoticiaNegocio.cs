using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Santuario.Entidade.Entities;
using Santuario.Negocio.Context;
using Santuario.Negocio.Helpers;
using Santuario.Negocio.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Santuario.Negocio.Negocios
{
    public class NoticiaNegocio : INoticiaNegocio
    {
        private readonly SantuarioDbContext _db;

        private const string PASTA_PUBLICA = "Noticia";

        // capa padrão 
        private const int TARGET_W = 1200;
        private const int TARGET_H = 700;

        public NoticiaNegocio(SantuarioDbContext db)
        {
            _db = db;
        }

        public Task<Noticia?> BuscarPorIdAsync(int id)
            => _db.Set<Noticia>()
                .Include(x => x.Comentarios)
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<List<Noticia>> ListarAsync(string? pesquisar, int? status)
        {
            var q = _db.Set<Noticia>()
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pesquisar))
            {
                var p = pesquisar.Trim();
                q = q.Where(x =>
                    x.Titulo.Contains(p) ||
                    x.Categoria.Contains(p) ||
                    (x.Subcategoria != null && x.Subcategoria.Contains(p)) ||
                    (x.Resumo != null && x.Resumo.Contains(p)));
            }

            if (status.HasValue)
                q = q.Where(x => x.Status == status.Value);

            // publicado primeiro, senão por criação
            return await q
                .OrderByDescending(x => x.DataPublicacao ?? x.DataCriacao)
                .ToListAsync();
        }

        public async Task<(bool ok, string? erro)> SalvarAsync(
            int id,
            string? titulo,
            string? categoria,
            string? subcategoria,
            string? resumo,
            string? conteudoJson,
            string? conteudoHtml,
            bool permiteComentarios,
            int status,
            string? imagemCapaUrlAtual,
            IFormFile? arquivoImagemCapa,
            int usuarioLogadoId,
            string webRootPath
        )
        {
            try
            {
                titulo = (titulo ?? "").Trim();
                categoria = (categoria ?? "").Trim();
                subcategoria = (subcategoria ?? "").Trim();
                resumo = (resumo ?? "").Trim();
                conteudoJson = (conteudoJson ?? "").Trim();
                conteudoHtml = (conteudoHtml ?? "").Trim();

                if (string.IsNullOrWhiteSpace(titulo))
                    return (false, "Informe o título.");

                if (string.IsNullOrWhiteSpace(categoria))
                    return (false, "Informe a categoria.");

                if (string.IsNullOrWhiteSpace(conteudoJson))
                    return (false, "Informe o conteúdo da notícia (JSON).");

                // normaliza status
                if (status < 0 || status > 2) status = 0;

                // regra: imagem só é obrigatória se publicar
                // (no editar, se já tem imagem no banco, pode publicar sem mandar uma nova)
                ImagemUploadHelper.EnsureFolder(webRootPath, PASTA_PUBLICA);

                var slugBase = GerarSlug(titulo);

          
                if (id == 0)
                {
                    // slug único
                    var slugFinal = await GerarSlugUnicoAsync(slugBase);

                    // publicar => precisa enviar imagem
                    if (status == 1 && (arquivoImagemCapa == null || arquivoImagemCapa.Length == 0))
                        return (false, "Para publicar a notícia é obrigatório selecionar a imagem de capa.");

                    string? imagemUrl = null;

                    // se enviou imagem, salva
                    if (arquivoImagemCapa != null && arquivoImagemCapa.Length > 0)
                    {
                        var rImg = await ImagemUploadHelper.SalvarOuSubstituirAsync(
                            arquivoImagem: arquivoImagemCapa,
                            webRootPath: webRootPath,
                            pastaPublica: PASTA_PUBLICA,
                            tituloBase: titulo,
                            targetW: TARGET_W,
                            targetH: TARGET_H,
                            currentImagemUrl: null
                        );

                        if (!rImg.ok)
                            return (false, rImg.erro);

                        imagemUrl = rImg.imagemUrlNova!;
                    }

                    var entity = new Noticia
                    {
                        Slug = slugFinal,
                        ImagemCapaUrl = imagemUrl, // ✅ pode ser null em rascunho
                        Titulo = titulo,
                        Categoria = categoria,
                        Subcategoria = string.IsNullOrWhiteSpace(subcategoria) ? null : subcategoria,
                        Resumo = string.IsNullOrWhiteSpace(resumo) ? null : resumo,
                        ConteudoJson = conteudoJson,
                        ConteudoHtml = string.IsNullOrWhiteSpace(conteudoHtml) ? null : conteudoHtml,
                        PermiteComentarios = permiteComentarios,
                        Status = status,
                        DataPublicacao = status == 1 ? DateTime.UtcNow : null,
                        DataCriacao = DateTime.UtcNow,
                        IdUsuarioCriacao = usuarioLogadoId
                    };

                    _db.Add(entity);
                    await _db.SaveChangesAsync();
                    return (true, null);
                }

                // EDITAR
                var db = await _db.Set<Noticia>().FirstOrDefaultAsync(x => x.Id == id);
                if (db == null)
                    return (false, "Notícia não encontrada.");

                // slug: recalcula a partir do título
                var slugNovo = slugBase;

                // se mudou, garante único
                if (!string.Equals(db.Slug, slugNovo, StringComparison.OrdinalIgnoreCase))
                {
                    if (await _db.Set<Noticia>().AnyAsync(x => x.Slug == slugNovo && x.Id != id))
                        slugNovo = await GerarSlugUnicoAsync(slugNovo);

                    db.Slug = slugNovo;
                }

                // publicar => precisa ter imagem (ou já ter no banco)
                if (status == 1)
                {
                    var temImagemAtual = !string.IsNullOrWhiteSpace(db.ImagemCapaUrl);
                    var temNovaImagem = arquivoImagemCapa != null && arquivoImagemCapa.Length > 0;

                    if (!temImagemAtual && !temNovaImagem)
                        return (false, "Para publicar a notícia é obrigatório selecionar a imagem de capa.");
                }

                var statusAnterior = db.Status;

                db.Titulo = titulo;
                db.Categoria = categoria;
                db.Subcategoria = string.IsNullOrWhiteSpace(subcategoria) ? null : subcategoria;
                db.Resumo = string.IsNullOrWhiteSpace(resumo) ? null : resumo;
                db.ConteudoJson = conteudoJson;
                db.ConteudoHtml = string.IsNullOrWhiteSpace(conteudoHtml) ? null : conteudoHtml;
                db.PermiteComentarios = permiteComentarios;
                db.Status = status;

                // se virou publicado agora, seta data
                if (statusAnterior != 1 && status == 1)
                    db.DataPublicacao = DateTime.UtcNow;

                db.DataAlteracao = DateTime.UtcNow;
                db.IdUsuarioAlteracao = usuarioLogadoId;

                // troca imagem se veio nova
                if (arquivoImagemCapa != null && arquivoImagemCapa.Length > 0)
                {
                    var rNova = await ImagemUploadHelper.SalvarOuSubstituirAsync(
                        arquivoImagem: arquivoImagemCapa,
                        webRootPath: webRootPath,
                        pastaPublica: PASTA_PUBLICA,
                        tituloBase: titulo,
                        targetW: TARGET_W,
                        targetH: TARGET_H,
                        currentImagemUrl: db.ImagemCapaUrl
                    );

                    if (!rNova.ok)
                        return (false, rNova.erro);

                    var caminhoFisicoAntigo = rNova.caminhoFisicoAntigo;

                    db.ImagemCapaUrl = rNova.imagemUrlNova!;

                    await _db.SaveChangesAsync();

                    // apaga antiga após salvar
                    ImagemUploadHelper.TryDeleteFile(caminhoFisicoAntigo);

                    return (true, null);
                }

                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch
            {
                return (false, "Não foi possível salvar a notícia.");
            }
        }

        public async Task<(bool ok, string? erro)> ArquivarAsync(int id, int usuarioLogadoId)
        {
            try
            {
                var db = await _db.Set<Noticia>().FirstOrDefaultAsync(x => x.Id == id);
                if (db == null) return (false, "Notícia não encontrada.");

                db.Status = 2; // Arquivado
                db.DataAlteracao = DateTime.UtcNow;
                db.IdUsuarioAlteracao = usuarioLogadoId;

                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch
            {
                return (false, "Não foi possível arquivar a notícia.");
            }
        }

        // =========================
        // Helpers internos (slug)
        // =========================

        private static string GerarSlug(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "noticia";

            var s = texto.Trim().ToLowerInvariant();

            // remove acentos
            s = s.Normalize(System.Text.NormalizationForm.FormD);
            s = new string(s.Where(c =>
                System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark
            ).ToArray());
            s = s.Normalize(System.Text.NormalizationForm.FormC);

            // remove caracteres inválidos
            s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
            s = Regex.Replace(s, @"\s+", " ").Trim();
            s = s.Replace(" ", "-");
            s = Regex.Replace(s, "-{2,}", "-").Trim('-');

            return string.IsNullOrWhiteSpace(s) ? "noticia" : s;
        }

        private async Task<string> GerarSlugUnicoAsync(string slugBase)
        {
            var slug = slugBase;
            var i = 2;

            while (await _db.Set<Noticia>().AnyAsync(x => x.Slug == slug))
            {
                slug = $"{slugBase}-{i}";
                i++;
                if (i > 9999) break;
            }

            return slug;
        }
    }
}