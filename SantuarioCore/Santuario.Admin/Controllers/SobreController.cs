using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Santuario.Admin.ViewModels.Sobre;
using Santuario.Entidade.Entities;
using Santuario.Negocio.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Santuario.Admin.Controllers
{
    [Authorize]
    public class SobreController : BaseController
    {
        private readonly ISobreNegocio _sobreNegocio;
        private readonly IWebHostEnvironment _env;

        public SobreController(ISobreNegocio sobreNegocio, IWebHostEnvironment env)
        {
            _sobreNegocio = sobreNegocio;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? pesquisar, bool? ativo)
        {
            var itens = await _sobreNegocio.ListarAsync(pesquisar, ativo);

            var vm = new IndexSobreViewModel
            {
                Pesquisar = pesquisar,
                Ativo = ativo,
                Itens = itens.Select(x => new SobreListaItemViewModel
                {
                    Id = x.Id,
                    Titulo1 = x.Titulo1,
                    Ativo = x.Ativo,
                    TotalTopicos = x.Topicos?.Count ?? 0
                }).ToList()
            };

            vm.StatusOptions = new()
            {
                new SelectListItem("🔄 Todos", "", selected: !ativo.HasValue),
                new SelectListItem("✅ Ativos", "true", selected: ativo == true),
                new SelectListItem("⛔ Inativos", "false", selected: ativo == false),
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CarregarModalSobre(int id)
        {
            var vm = new ModalSobreViewModel();

            if (id > 0)
            {
                var db = await _sobreNegocio.BuscarPorIdAsync(id);
                if (db == null) return NotFound();

                vm.Item.Id = db.Id;
                vm.Item.ImagemUrl = db.ImagemUrl;
                vm.Item.Titulo1 = db.Titulo1;
                vm.Item.Descricao1 = db.Descricao1;
                vm.Item.Titulo2 = db.Titulo2;
                vm.Item.Descricao2 = db.Descricao2;
                vm.Item.Ativo = db.Ativo;

                vm.Topicos = (db.Topicos ?? Enumerable.Empty<SobreTopico>())
                    .OrderBy(x => x.Ordem)
                    .Select(x => new SobreTopicoItemViewModel
                    {
                        Ordem = x.Ordem,
                        Texto = x.Texto,
                        Ativo = x.Ativo
                    }).ToList();
            }

            if (vm.Topicos == null || vm.Topicos.Count == 0)
                vm.Topicos.Add(new SobreTopicoItemViewModel { Ordem = 1 });

            return PartialView("_ModalSobre", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarSobre(ModalSobreViewModel model, IFormFile? imagemSobre)
        {
            model ??= new ModalSobreViewModel();
            model.Item ??= new SobreItemViewModel();
            model.Topicos ??= new();

            ModelState.Remove("Item.ImagemUrl");

            if (!ModelState.IsValid)
                return PartialView("_ModalSobre", model);

            string imagemAtual = model.Item.ImagemUrl ?? "";

            if (model.Item.Id > 0)
            {
                var db = await _sobreNegocio.BuscarPorIdAsync(model.Item.Id);
                if (db == null)
                {
                    ModelState.AddModelError("", "Registro não encontrado.");
                    return PartialView("_ModalSobre", model);
                }
                imagemAtual = db.ImagemUrl ?? "";
                model.Item.ImagemUrl = imagemAtual;
            }

            var topicos = (model.Topicos ?? Enumerable.Empty<SobreTopicoItemViewModel>())
                .Select(t => new SobreTopico
                {
                    Ordem = t.Ordem <= 0 ? 1 : t.Ordem,
                    Texto = (t.Texto ?? "").Trim(),
                    Ativo = t.Ativo
                })
                .ToList();

            var r = await _sobreNegocio.SalvarAsync(
                id: model.Item.Id,
                titulo1: model.Item.Titulo1,
                descricao1: model.Item.Descricao1,
                titulo2: model.Item.Titulo2,
                descricao2: model.Item.Descricao2,
                imagemUrlAtual: imagemAtual,
                arquivoImagem: imagemSobre,
                ativo: model.Item.Ativo,
                topicos: topicos,
                usuarioLogadoId: UsuarioLogadoId,
                webRootPath: _env.WebRootPath
            );

            if (!r.ok)
            {
                ModelState.AddModelError("", r.erro ?? "Erro ao salvar.");
                return PartialView("_ModalSobre", model);
            }

            return Json(new { success = true, message = "Sobre salvo com sucesso!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var r = await _sobreNegocio.InativarAsync(id, UsuarioLogadoId);

            if (!r.ok)
                return Json(new { success = false, message = r.erro ?? "Não foi possível inativar." });

            return Json(new { success = true, message = "Sobre inativado com sucesso!" });
        }
    }
}