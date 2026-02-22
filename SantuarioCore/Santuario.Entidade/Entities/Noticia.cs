using Santuario.Entidade.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Santuario.Entidade.Entities
{
    [Table("noticia")]
    public class Noticia : EntidadeAuditoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Slug { get; set; }

        public string ImagemCapaUrl { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]
        public string Categoria { get; set; }

        public string Subcategoria { get; set; }

        /// <summary>Pequeno resumo para listagem</summary>
        public string Resumo { get; set; }

        /// <summary>Conteúdo rico (Delta/Editor) em JSON</summary>
        [Required]
        public string ConteudoJson { get; set; }

        /// <summary>HTML gerado do editor</summary>
        public string ConteudoHtml { get; set; }

        public bool PermiteComentarios { get; set; } = true;

        /// <summary>0=Rascunho, 1=Publicado, 2=Arquivado</summary>
        public int Status { get; set; } = 0;

        public DateTime? DataPublicacao { get; set; }

        public virtual ICollection<NoticiaComentario> Comentarios { get; set; } = new List<NoticiaComentario>();
    }
}