using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Santuario.Admin.ViewModels.Usuario;
using Santuario.Entidade.Entities;
using Santuario.Negocio.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Santuario.Admin.Controllers
{
    [Authorize] 
    public class UsuarioController : BaseController
    {
        private readonly IUsuarioNegocio _usuarioNegocio;

        public UsuarioController(IUsuarioNegocio usuarioNegocio)
        {
            _usuarioNegocio = usuarioNegocio;
        }


       
        [HttpGet]
        public async Task<IActionResult> Index(string? pesquisar, bool? ativo, TipoUsuario? tipo)
        {
            var lista = await _usuarioNegocio.ListarAsync(pesquisar, ativo, tipo);

            var vm = new IndexUsuarioViewModel
            {
                Pesquisar = pesquisar,
                Ativo = ativo,
                Tipo = tipo,
                Usuarios = lista.Select(x => new UsuarioListaItemViewModel
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    Login = x.Login,
                    Ativo = x.Ativo,
                    Tipo = x.Tipo
                }).ToList()
            };

            vm.StatusOptions = new()
            {
                new SelectListItem("🔄 Todos", "", selected: !ativo.HasValue),
                new SelectListItem("✅ Ativos", "true", selected: ativo == true),
                new SelectListItem("⛔ Inativos", "false", selected: ativo == false),
            };

            vm.TipoOptions = Enum.GetValues(typeof(TipoUsuario))
                .Cast<TipoUsuario>()
                .Select(t => new SelectListItem(
                    text: t.ToString(),
                    value: ((int)t).ToString(),
                    selected: tipo.HasValue && tipo.Value == t
                ))
                .Prepend(new SelectListItem("🔄 Todos", "", selected: !tipo.HasValue))
                .ToList();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CarregarModalUsuario(int id)
        {
            var vm = new ModalUsuarioViewModel();

            if (id > 0)
            {
                var u = await _usuarioNegocio.BuscarPorIdAsync(id);
                if (u == null)
                    return NotFound();

                vm.Usuario = new UsuarioEditViewModel
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Login = u.Login,
                    Tipo = u.Tipo,
                    Ativo = u.Ativo,
                    Senha = null // nunca envia senha
                };
            }

            return PartialView("_ModalUsuario", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarUsuario(ModalUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_ModalUsuario", model);

            var r = await _usuarioNegocio.SalvarAsync(
                id: model.Usuario.Id,
                nome: model.Usuario.Nome,
                login: model.Usuario.Login,
                senha: model.Usuario.Senha,
                tipo: model.Usuario.Tipo,
                ativo: model.Usuario.Ativo,
                usuarioLogadoId: UsuarioLogadoId 
            );

            if (!r.ok)
                return Json(new { success = false, message = r.erro ?? "Erro ao salvar." });

            return Json(new { success = true, message = "Usuário salvo com sucesso!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var r = await _usuarioNegocio.InativarAsync(id, UsuarioLogadoId);

            if (!r.ok)
                return Json(new { success = false, message = r.erro ?? "Não foi possível inativar." });

            return Json(new { success = true, message = "Usuário inativado com sucesso!" });
        }
    }
}