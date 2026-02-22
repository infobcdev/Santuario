using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Santuario.Admin.ViewModels.Sobre
{
    public class IndexSobreViewModel
    {
        public string Pesquisar { get; set; }
        public bool? Ativo { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();

        public List<SobreListaItemViewModel> Itens { get; set; } = new();
    }

    public class SobreListaItemViewModel
    {
        public int Id { get; set; }
        public string Titulo1 { get; set; }
        public bool Ativo { get; set; }
        public int TotalTopicos { get; set; }
    }
}