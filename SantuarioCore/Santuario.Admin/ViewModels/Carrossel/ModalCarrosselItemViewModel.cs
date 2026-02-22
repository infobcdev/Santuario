using System.ComponentModel.DataAnnotations;

namespace Santuario.Admin.ViewModels.Carrossel
{
    public class ModalCarrosselItemViewModel
    {
        public CarrosselEditViewModel Item { get; set; } = new();
    }

    public class CarrosselEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe a ordem.")]
        public int Ordem { get; set; } = 1;

        [Required(ErrorMessage = "Informe o título.")]
        public string Titulo { get; set; } = "";

        public string? Descricao { get; set; }

        public string? ImagemUrl { get; set; }

        public bool Ativo { get; set; } = true;
    }
}