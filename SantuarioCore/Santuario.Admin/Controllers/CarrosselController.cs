using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Santuario.Admin.ViewModels.Carrossel;
using Santuario.Negocio.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Santuario.Admin.Controllers
{
    [Authorize]
    public class CarrosselController : BaseController
    {
        private readonly ICarrosselNegocio _carrosselNegocio;
        private readonly IWebHostEnvironment _env;

        public CarrosselController(ICarrosselNegocio carrosselNegocio, IWebHostEnvironment env)
        {
            _carrosselNegocio = carrosselNegocio;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? pesquisar, bool? ativo)
        {
            var lista = await _carrosselNegocio.ListarAsync(pesquisar, ativo);

            var vm = new IndexCarrosselViewModel
            {
                Pesquisar = pesquisar,
                Ativo = ativo,
                Itens = lista.Select(x => new CarrosselListaItemViewModel
                {
                    Id = x.Id,
                    Ordem = x.Ordem,
                    Titulo = x.Titulo,
                    Descricao = x.Descricao,
                    ImagemUrl = x.ImagemUrl,
                    Ativo = x.Ativo
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
        public async Task<IActionResult> CarregarModalCarrossel(int id)
        {
            var vm = new ModalCarrosselItemViewModel();

            if (id > 0)
            {
                var item = await _carrosselNegocio.BuscarPorIdAsync(id);
                if (item == null)
                    return NotFound();

                vm.Item.Id = item.Id;
                vm.Item.Ordem = item.Ordem;
                vm.Item.Titulo = item.Titulo;
                vm.Item.Descricao = item.Descricao;
                vm.Item.ImagemUrl = item.ImagemUrl;
                vm.Item.Ativo = item.Ativo;
            }

            return PartialView("_ModalCarrosselItem", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarCarrossel(
            ModalCarrosselItemViewModel model,
            IFormFile? imagemCarrossel
        )
        {
            model ??= new ModalCarrosselItemViewModel();
            model.Item ??= new CarrosselEditViewModel();

            ModelState.Remove("Item.ImagemUrl");

            if (!ModelState.IsValid)
                return PartialView("_ModalCarrosselItem", model);

            bool precisaImagem = false;

            if (model.Item.Id == 0)
            {
                precisaImagem = true;
            }
            else
            {
                var itemDb = await _carrosselNegocio.BuscarPorIdAsync(model.Item.Id);
                if (itemDb == null)
                {
                    ModelState.AddModelError("", "Item não encontrado.");
                    return PartialView("_ModalCarrosselItem", model);
                }

                precisaImagem = string.IsNullOrWhiteSpace(itemDb.ImagemUrl);
                model.Item.ImagemUrl = itemDb.ImagemUrl ?? "";
            }

            if (precisaImagem && (imagemCarrossel == null || imagemCarrossel.Length == 0))
            {
                ModelState.AddModelError("", "Selecione uma imagem (PNG ou JPG).");
                return PartialView("_ModalCarrosselItem", model);
            }

            var r = await _carrosselNegocio.SalvarAsync(
                id: model.Item.Id,
                ordem: model.Item.Ordem,
                titulo: model.Item.Titulo,
                descricao: model.Item.Descricao,
                imagemUrlAtual: model.Item.ImagemUrl, 
                arquivoImagem: imagemCarrossel,      
                ativo: model.Item.Ativo,
                usuarioLogadoId: UsuarioLogadoId,
                webRootPath: _env.WebRootPath
            );

            if (!r.ok)
                return Json(new { success = false, message = r.erro ?? "Erro ao salvar." });

            return Json(new { success = true, message = "Item do carrossel salvo com sucesso!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var r = await _carrosselNegocio.InativarAsync(id, UsuarioLogadoId);

            if (!r.ok)
                return Json(new { success = false, message = r.erro ?? "Não foi possível inativar." });

            return Json(new { success = true, message = "Item inativado com sucesso!" });
        }
    }
}