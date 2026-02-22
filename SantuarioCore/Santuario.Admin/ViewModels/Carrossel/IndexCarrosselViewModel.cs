using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Santuario.Admin.ViewModels.Carrossel
{
    public class IndexCarrosselViewModel
    {
        public string? Pesquisar { get; set; }
        public bool? Ativo { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();

        public List<CarrosselListaItemViewModel> Itens { get; set; } = new();
    }

    public class CarrosselListaItemViewModel
    {
        public int Id { get; set; }
        public int Ordem { get; set; }
        public string Titulo { get; set; } = "";
        public string? Descricao { get; set; }
        public string ImagemUrl { get; set; } = "";
        public bool Ativo { get; set; }
    }
}