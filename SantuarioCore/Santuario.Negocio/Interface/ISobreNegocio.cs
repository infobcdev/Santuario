using Microsoft.AspNetCore.Http;
using Santuario.Entidade.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Santuario.Negocio.Interface
{
    public interface ISobreNegocio
    {
        Task<List<Sobre>> ListarAsync(string pesquisar, bool? ativo);
        Task<Sobre> BuscarPorIdAsync(int id);

        Task<(bool ok, string? erro)> SalvarAsync(
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
        );

        Task<(bool ok, string? erro)> InativarAsync(int id, int usuarioLogadoId);
    }
}