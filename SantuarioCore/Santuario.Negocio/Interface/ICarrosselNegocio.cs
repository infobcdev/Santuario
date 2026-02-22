using Microsoft.AspNetCore.Http;
using Santuario.Entidade.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Santuario.Negocio.Interfaces
{
    public interface ICarrosselNegocio
    {
        Task<List<CarrosselItem>> ListarAsync(string? pesquisar, bool? ativo);
        Task<CarrosselItem?> BuscarPorIdAsync(int id);

        Task<(bool ok, string? erro)> SalvarAsync(
              int id,
              int ordem,
              string titulo,
              string? descricao,
              string? imagemUrlAtual,
              IFormFile? arquivoImagem,
              bool ativo,
              int usuarioLogadoId,
              string webRootPath
          );

        Task<(bool ok, string? erro)> InativarAsync(int id, int usuarioLogadoId);
    }
}