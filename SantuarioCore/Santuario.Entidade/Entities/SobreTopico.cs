using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Santuario.Entidade.Entities.Base;

namespace Santuario.Entidade.Entities
{
    [Table("sobre_topico")]
    public class SobreTopico : EntidadeAuditoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int IdSobre { get; set; }

        [Required]
        public int Ordem { get; set; } = 1;

        [Required]
        public string Texto { get; set; }

        [Required]
        public bool Ativo { get; set; } = true;

        [ForeignKey(nameof(IdSobre))]
        public virtual Sobre Sobre { get; set; }
    }
}