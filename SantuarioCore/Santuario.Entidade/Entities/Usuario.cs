using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Santuario.Entidade.Entities.Base;

namespace Santuario.Entidade.Entities
{
    public class Usuario : EntidadeAuditoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Nome { get; set; }

        [Required, MaxLength(80)]
        public string Login { get; set; }

        [Required]
        public byte[] SenhaHash { get; set; } = Array.Empty<byte>();
        
        [Required]
        public byte[] SenhaSalt { get; set; } = Array.Empty<byte>();

        [Required]
        public bool Ativo { get; set; } = true;

        [Required]
        public TipoUsuario Tipo { get; set; } = TipoUsuario.Administrador;
    }

    public enum TipoUsuario
    {
        Administrador = 1,
        Editor = 2
    }
}