using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Santuario.Admin.ViewModels.Login;
using Santuario.Negocio.Interface;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Santuario.Admin.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly ILoginNegocio _loginNegocio;

        public LoginController(ILoginNegocio loginNegocio)
        {
            _loginNegocio = loginNegocio;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Se já estiver logado, redireciona para Home
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MensagemErro = "Informe login e senha.";
                return View(model);
            }

            var usuario = await _loginNegocio.AutenticarAsync(model.Login, model.Senha);

            if (usuario == null)
            {
                ViewBag.MensagemErro = "Login ou senha inválidos.";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome ?? usuario.Login),
                new Claim(ClaimTypes.Role, usuario.Tipo.ToString()),
                new Claim("tipoUsuario", ((int)usuario.Tipo).ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true
                }
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Login");
        }
    }
}