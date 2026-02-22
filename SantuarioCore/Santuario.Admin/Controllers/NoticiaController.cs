using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Santuario.Admin.ViewModels.Noticia;
using Santuario.Negocio.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Santuario.Admin.Controllers
{
    [Authorize]
    public class NoticiaController : BaseController
    {
        private readonly INoticiaNegocio _noticiaNegocio;
        private readonly IWebHostEnvironment _env;

        public NoticiaController(INoticiaNegocio noticiaNegocio, IWebHostEnvironment env)
        {
            _noticiaNegocio = noticiaNegocio;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? pesquisar, int? status)
        {
            var lista = await _noticiaNegocio.ListarAsync(pesquisar, status);

            var vm = new IndexNoticiaViewModel
            {
                Pesquisar = pesquisar,
                Status = status,
                Itens = lista.Select(x => new NoticiaListaItemViewModel
                {
                    Id = x.Id,
                    Titulo = x.Titulo,
                    Categoria = x.Categoria,
                    Subcategoria = x.Subcategoria,
                    Status = x.Status,
                    DataPublicacao = x.DataPublicacao
                }).ToList()
            };

            vm.StatusOptions = new()
            {
                new SelectListItem("🔄 Todos", "", selected: !status.HasValue),
                new SelectListItem("📝 Rascunho", "0", selected: status == 0),
                new SelectListItem("✅ Publicado", "1", selected: status == 1),
                new SelectListItem("📦 Arquivado", "2", selected: status == 2),
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CarregarModalNoticia(int id)
        {
            var vm = new ModalNoticiaViewModel();

            if (id > 0)
            {
                var db = await _noticiaNegocio.BuscarPorIdAsync(id);
                if (db == null) return NotFound();

                vm.Item.Id = db.Id;
                vm.Item.ImagemCapaUrl = db.ImagemCapaUrl;
                vm.Item.Titulo = db.Titulo;
                vm.Item.Categoria = db.Categoria;
                vm.Item.Subcategoria = db.Subcategoria;
                vm.Item.Resumo = db.Resumo;
                vm.Item.ConteudoJson = db.ConteudoJson;
                vm.Item.ConteudoHtml = db.ConteudoHtml;
                vm.Item.PermiteComentarios = db.PermiteComentarios;
                vm.Item.Status = db.Status;
            }

            vm.StatusOptions = new()
            {
                new SelectListItem("📝 Rascunho", "0", selected: vm.Item.Status == 0),
                new SelectListItem("✅ Publicado", "1", selected: vm.Item.Status == 1),
                new SelectListItem("📦 Arquivado", "2", selected: vm.Item.Status == 2),
            };

            return PartialView("_ModalNoticia", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarNoticia(ModalNoticiaViewModel model, IFormFile? imagemCapa)
        {
            model ??= new ModalNoticiaViewModel();
            model.Item ??= new NoticiaEditViewModel();

            //  imagem só é obrigatória na publicação (regra no negócio)
            ModelState.Remove("Item.ImagemCapaUrl");

            if (!ModelState.IsValid)
            {
                model.StatusOptions = new()
                {
                    new SelectListItem("📝 Rascunho", "0", selected: model.Item.Status == 0),
                    new SelectListItem("✅ Publicado", "1", selected: model.Item.Status == 1),
                    new SelectListItem("📦 Arquivado", "2", selected: model.Item.Status == 2),
                };

                return PartialView("_ModalNoticia", model);
            }

            var r = await _noticiaNegocio.SalvarAsync(
                id: model.Item.Id,
                titulo: model.Item.Titulo,
                categoria: model.Item.Categoria,
                subcategoria: model.Item.Subcategoria,
                resumo: model.Item.Resumo,
                conteudoJson: model.Item.ConteudoJson,
                conteudoHtml: model.Item.ConteudoHtml,
                permiteComentarios: model.Item.PermiteComentarios,
                status: model.Item.Status,
                imagemCapaUrlAtual: model.Item.ImagemCapaUrl,
                arquivoImagemCapa: imagemCapa,
                usuarioLogadoId: UsuarioLogadoId,
                webRootPath: _env.WebRootPath
            );

            if (!r.ok)
                return Json(new { success = false, message = r.erro ?? "Erro ao salvar." });

            return Json(new { success = true, message = "Notícia salva com sucesso!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Arquivar(int id)
        {
            var r = await _noticiaNegocio.ArquivarAsync(id, UsuarioLogadoId);

            if (!r.ok)
                return Json(new { success = false, message = r.erro ?? "Não foi possível arquivar." });

            return Json(new { success = true, message = "Notícia arquivada com sucesso!" });
        }
    }
}