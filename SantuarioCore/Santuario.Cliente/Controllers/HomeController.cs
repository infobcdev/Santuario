using Microsoft.AspNetCore.Mvc;

namespace Santuario.Cliente.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Santuário Nossa Senhora da Conceição Aparecida";
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
