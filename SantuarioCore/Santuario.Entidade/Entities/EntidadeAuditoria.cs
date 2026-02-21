using System;
using System.ComponentModel.DataAnnotations;

namespace Santuario.Entidade.Entities.Base
{
    /// <summary>
    /// Classe base para controle de auditoria (criação e alteração).
    /// </summary>
    public abstract class EntidadeAuditoria
    {
        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime? DataAlteracao { get; set; }

        public int? IdUsuarioCriacao { get; set; }

        public int? IdUsuarioAlteracao { get; set; }
    }
}