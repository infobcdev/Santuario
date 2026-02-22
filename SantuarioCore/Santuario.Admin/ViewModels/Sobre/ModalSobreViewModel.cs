using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Santuario.Admin.ViewModels.Sobre
{
    public class ModalSobreViewModel
    {
        public SobreItemViewModel Item { get; set; } = new();
        public List<SobreTopicoItemViewModel> Topicos { get; set; } = new();
    }

    public class SobreItemViewModel
    {
        public int Id { get; set; }

        public string ImagemUrl { get; set; }

        [Required(ErrorMessage = "Título 1 é obrigatório.")]
        public string Titulo1 { get; set; }

        [Required(ErrorMessage = "Descrição 1 é obrigatória.")]
        public string Descricao1 { get; set; }

        public string Titulo2 { get; set; }
        public string Descricao2 { get; set; }

        public bool Ativo { get; set; } = true;
    }

    public class SobreTopicoItemViewModel
    {
        public int Ordem { get; set; } = 1;

        [Required(ErrorMessage = "Texto do tópico é obrigatório.")]
        public string Texto { get; set; }

        public bool Ativo { get; set; } = true;
    }
}