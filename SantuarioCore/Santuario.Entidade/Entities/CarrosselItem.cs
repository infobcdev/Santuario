using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Santuario.Entidade.Entities.Base;

namespace Santuario.Entidade.Entities
{
    [Table("carrossel_item")]
    public class CarrosselItem : EntidadeAuditoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int Ordem { get; set; } = 1;

        [Required]
        public string ImagemUrl { get; set; }

        [Required]
        public string Titulo { get; set; }

        public string Descricao { get; set; }

        [Required]
        public bool Ativo { get; set; } = true;
    }
}