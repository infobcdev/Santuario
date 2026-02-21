using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Santuario.Admin.ViewModels;
using Santuario.Entidade.ViewModels;

namespace Santuario.Admin.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [HttpGet("/Error/{code:int}")]
        public IActionResult Index(int code, string? originalPath = null, string? requestId = null)
        {
            var (titulo, msg) = code switch
            {
                404 => ("Página não encontrada", "A página que você tentou acessar não existe ou foi movida."),
                401 => ("Acesso restrito", "Você precisa entrar para continuar."),
                403 => ("Acesso negado", "Você não tem permissão para acessar esta área."),
                504 => ("Tempo excedido", "A operação demorou mais que o esperado. Tente novamente."),
                _ => ("Ops... aconteceu um erro", "Não foi possível concluir sua solicitação. Tente novamente em instantes.")
            };

            ViewData["Title"] = titulo;

            var vm = new ErrorViewModel
            {
                StatusCode = code,
                Mensagem = msg,
                RequestId = requestId ?? HttpContext.TraceIdentifier,
                OriginalPath = originalPath
            };

            return View("Error", vm);
        }
    }
}