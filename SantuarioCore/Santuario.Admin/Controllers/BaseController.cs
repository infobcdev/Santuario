using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Santuario.Admin.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int UsuarioLogadoId
        {
            get
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                return claim != null ? int.Parse(claim.Value) : 0;
            }
        }
    }
}
