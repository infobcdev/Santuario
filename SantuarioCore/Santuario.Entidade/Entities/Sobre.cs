using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Santuario.Entidade.Entities.Base;

namespace Santuario.Entidade.Entities
{
    [Table("sobre")]
    public class Sobre : EntidadeAuditoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ImagemUrl { get; set; }

        [Required]
        public string Titulo1 { get; set; }

        [Required]
        public string Descricao1 { get; set; }

        public string Titulo2 { get; set; }
        public string Descricao2 { get; set; }

        [Required]
        public bool Ativo { get; set; } = true;

        public virtual ICollection<SobreTopico> Topicos { get; set; } = new List<SobreTopico>();
    }
}