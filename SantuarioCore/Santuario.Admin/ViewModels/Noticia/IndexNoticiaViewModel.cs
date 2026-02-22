using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Santuario.Admin.ViewModels.Noticia
{
    public class IndexNoticiaViewModel
    {
        public string? Pesquisar { get; set; }
        public int? Status { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();

        public List<NoticiaListaItemViewModel> Itens { get; set; } = new();
    }

    public class NoticiaListaItemViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string? Subcategoria { get; set; }
        public int Status { get; set; }
        public System.DateTime? DataPublicacao { get; set; }
    }
}