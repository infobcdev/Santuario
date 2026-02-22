using Santuario.Entidade.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Santuario.Negocio.Interfaces
{
    public interface IUsuarioNegocio
    {
        Task<List<Usuario>> ListarAsync(string? pesquisar, bool? ativo, TipoUsuario? tipo);
        Task<Usuario?> BuscarPorIdAsync(int id);
        Task<Usuario?> BuscarPorLoginAsync(string login, int? ignorarId = null);

        Task<(bool ok, string? erro)> SalvarAsync(
            int id,
            string nome,
            string login,
            string? senha,
            TipoUsuario tipo,
            bool ativo,
            int usuarioLogadoId
        );

        Task<(bool ok, string? erro)> InativarAsync(int id, int usuarioLogadoId);
    }
}