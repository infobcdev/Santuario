using Santuario.Entidade.Entities;
using System.ComponentModel.DataAnnotations;

namespace Santuario.Admin.ViewModels.Usuario
{
    public class ModalUsuarioViewModel
    {
        public UsuarioEditViewModel Usuario { get; set; } = new();
    }

    public class UsuarioEditViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Nome { get; set; } = "";

        [Required, MaxLength(80)]
        public string Login { get; set; } = "";

        // ✅ Para editar: se deixar vazio, mantém a senha atual
        [MinLength(4, ErrorMessage = "A senha deve ter no mínimo 4 caracteres.")]
        public string? Senha { get; set; }

        [Required]
        public TipoUsuario Tipo { get; set; } = TipoUsuario.Administrador;

        public bool Ativo { get; set; } = true;
    }
}