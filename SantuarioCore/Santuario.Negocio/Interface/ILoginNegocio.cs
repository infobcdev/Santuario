using Santuario.Entidade.Entities;
using System.Threading.Tasks;

namespace Santuario.Negocio.Interface
{
    public interface ILoginNegocio
    {
        Task<Usuario?> AutenticarAsync(string login, string senha);
    }
}