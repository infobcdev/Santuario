using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Santuario.Admin.ViewModels.Noticia
{
    public class ModalNoticiaViewModel
    {
        public NoticiaEditViewModel Item { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class NoticiaEditViewModel
    {
        public int Id { get; set; }

        public string? ImagemCapaUrl { get; set; } // ✅ obrigatória só na publicação (regra no negócio)

        [Required(ErrorMessage = "Informe o título.")]
        [MaxLength(250)]
        public string Titulo { get; set; } = "";

        [Required(ErrorMessage = "Informe a categoria.")]
        [MaxLength(150)]
        public string Categoria { get; set; } = "";

        [MaxLength(150)]
        public string? Subcategoria { get; set; }

        public string? Resumo { get; set; }

        // obrigatório no banco e também no form (mesmo que seja "{}" / delta vazio)
        [Required(ErrorMessage = "Informe o conteúdo da notícia.")]
        public string ConteudoJson { get; set; } = "";

        public string? ConteudoHtml { get; set; }

        public bool PermiteComentarios { get; set; } = true;

        // 0=Rascunho, 1=Publicado, 2=Arquivado
        public int Status { get; set; } = 0;
    }
}