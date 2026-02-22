using Microsoft.AspNetCore.Mvc.Rendering;
using Santuario.Entidade.Entities;
using System.Collections.Generic;

namespace Santuario.Admin.ViewModels.Usuario
{
    public class IndexUsuarioViewModel
    {
        public string? Pesquisar { get; set; }
        public bool? Ativo { get; set; } // null=Todos, true=Ativos, false=Inativos
        public TipoUsuario? Tipo { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<SelectListItem> TipoOptions { get; set; } = new();

        public List<UsuarioListaItemViewModel> Usuarios { get; set; } = new();
    }

    public class UsuarioListaItemViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Login { get; set; } = "";
        public bool Ativo { get; set; }
        public TipoUsuario Tipo { get; set; }
    }
}