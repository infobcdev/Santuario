using Santuario.Entidade.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Santuario.Entidade.Entities
{
    [Table("noticia_comentario")]
    public class NoticiaComentario : EntidadeAuditoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int IdNoticia { get; set; }

        [Required]
        public string UsuarioProvider { get; set; } = "Google";

        [Required]
        public string UsuarioProviderId { get; set; }   // claim "sub"

        [Required]
        public string UsuarioNome { get; set; }

        public string UsuarioEmail { get; set; }

        [Required]
        public string Texto { get; set; }

        [Required]
        public bool Ativo { get; set; } = true;

        [ForeignKey(nameof(IdNoticia))]
        public virtual Noticia Noticia { get; set; }
    }
}