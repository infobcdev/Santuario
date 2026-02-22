using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Santuario.Entidade.Entities;
using Santuario.Negocio.Context;
using Santuario.Negocio.Helpers; 
using Santuario.Negocio.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Santuario.Negocio.Servicos
{
    public class CarrosselNegocio : ICarrosselNegocio
    {
        private readonly SantuarioDbContext _db;

        // padrão fixo do carrossel
        private const int TARGET_W = 1920;
        private const int TARGET_H = 720;

        private const string PASTA_PUBLICA = "Carrossel";

        public CarrosselNegocio(SantuarioDbContext db)
        {
            _db = db;
        }

        public Task<CarrosselItem?> BuscarPorIdAsync(int id)
            => _db.Set<CarrosselItem>().FirstOrDefaultAsync(x => x.Id == id);

        public async Task<List<CarrosselItem>> ListarAsync(string? pesquisar, bool? ativo)
        {
            var q = _db.Set<CarrosselItem>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(pesquisar))
            {
                var p = pesquisar.Trim();
                q = q.Where(x =>
                    x.Titulo.Contains(p) ||
                    (x.Descricao != null && x.Descricao.Contains(p)));
            }

            if (ativo.HasValue)
                q = q.Where(x => x.Ativo == ativo.Value);

            return await q
                .OrderBy(x => x.Ordem)
                .ToListAsync();
        }

        public async Task<(bool ok, string? erro)> SalvarAsync(
            int id,
            int ordem,
            string titulo,
            string? descricao,
            string? imagemUrlAtual,
            IFormFile? arquivoImagem,
            bool ativo,
            int usuarioLogadoId,
            string webRootPath
        )
        {
            try
            {
                titulo = (titulo ?? "").Trim();
                descricao = (descricao ?? "").Trim();

                if (ordem <= 0) ordem = 1;

                if (string.IsNullOrWhiteSpace(titulo))
                    return (false, "Informe o título.");

                // garante pasta física (wwwroot/Carrossel)
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
                        tituloBase: titulo,
                        targetW: TARGET_W,
                        targetH: TARGET_H,
                        currentImagemUrl: null
                    );

                    if (!rImg.ok)
                        return (false, rImg.erro);

                    var entity = new CarrosselItem
                    {
                        Ordem = ordem,
                        Titulo = titulo,
                        Descricao = descricao,
                        Ativo = ativo,
                        ImagemUrl = rImg.imagemUrlNova!,
                        DataCriacao = DateTime.UtcNow,
                        IdUsuarioCriacao = usuarioLogadoId
                    };

                    _db.Add(entity);
                    await _db.SaveChangesAsync();
                    return (true, null);
                }

                var entityEdit = await _db.Set<CarrosselItem>().FirstOrDefaultAsync(x => x.Id == id);
                if (entityEdit == null)
                    return (false, "Item não encontrado.");

                entityEdit.Ordem = ordem;
                entityEdit.Titulo = titulo;
                entityEdit.Descricao = descricao;
                entityEdit.Ativo = ativo;
                entityEdit.DataAlteracao = DateTime.UtcNow;
                entityEdit.IdUsuarioAlteracao = usuarioLogadoId;

                // se não veio arquivo novo, só salva campos
                if (arquivoImagem == null || arquivoImagem.Length == 0)
                {
                    // se por acaso o item não tem imagem no banco, exige
                    if (string.IsNullOrWhiteSpace(entityEdit.ImagemUrl))
                        return (false, "Selecione uma imagem (PNG ou JPG).");

                    await _db.SaveChangesAsync();
                    return (true, null);
                }

                // veio arquivo novo => troca imagem
                var rNova = await ImagemUploadHelper.SalvarOuSubstituirAsync(
                    arquivoImagem: arquivoImagem,
                    webRootPath: webRootPath,
                    pastaPublica: PASTA_PUBLICA,
                    tituloBase: titulo,
                    targetW: TARGET_W,
                    targetH: TARGET_H,
                    currentImagemUrl: entityEdit.ImagemUrl
                );

                if (!rNova.ok)
                    return (false, rNova.erro);

                var caminhoFisicoAntigo = rNova.caminhoFisicoAntigo;

                entityEdit.ImagemUrl = rNova.imagemUrlNova!;

                await _db.SaveChangesAsync();

                // apaga antiga só após salvar no banco
                ImagemUploadHelper.TryDeleteFile(caminhoFisicoAntigo);

                return (true, null);
            }
            catch
            {
                return (false, "Não foi possível salvar o item do carrossel.");
            }
        }

        public async Task<(bool ok, string? erro)> InativarAsync(int id, int usuarioLogadoId)
        {
            try
            {
                var entity = await _db.Set<CarrosselItem>().FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    return (false, "Item não encontrado.");

                entity.Ativo = false;
                entity.DataAlteracao = DateTime.UtcNow;
                entity.IdUsuarioAlteracao = usuarioLogadoId;

                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch
            {
                return (false, "Não foi possível inativar o item.");
            }
        }
    }
}