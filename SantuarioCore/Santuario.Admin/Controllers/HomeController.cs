using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Santuario.Admin.Controllers
{
    [Authorize] 
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}