using Microsoft.AspNetCore.Http;
using Santuario.Entidade.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Santuario.Negocio.Interface
{
    public interface INoticiaNegocio
    {
        Task<Noticia?> BuscarPorIdAsync(int id);

        Task<List<Noticia>> ListarAsync(string? pesquisar, int? status);

        Task<(bool ok, string? erro)> SalvarAsync(
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
        );

        Task<(bool ok, string? erro)> ArquivarAsync(int id, int usuarioLogadoId);
    }
}