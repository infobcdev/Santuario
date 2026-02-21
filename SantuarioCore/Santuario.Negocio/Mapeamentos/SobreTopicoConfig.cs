// ===================================================
// Santuario.Negocio/Mapeamentos/SobreTopicoConfig.cs
// ===================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Santuario.Entidade.Entities;

namespace Santuario.Negocio.Mapeamentos
{
    public class SobreTopicoConfig : IEntityTypeConfiguration<SobreTopico>
    {
        public void Configure(EntityTypeBuilder<SobreTopico> e)
        {
            e.ToTable("sobre_topico");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
              .HasColumnName("id")
              .ValueGeneratedOnAdd()
              .UseIdentityByDefaultColumn();

            e.Property(x => x.IdSobre).HasColumnName("idsobre");
            e.Property(x => x.Ordem).HasColumnName("ordem");
            e.Property(x => x.Texto).HasColumnName("texto");
            e.Property(x => x.Ativo).HasColumnName("ativo");

            // Auditoria (base)
            e.Property(x => x.DataCriacao).HasColumnName("datacriacao");
            e.Property(x => x.DataAlteracao).HasColumnName("dataalteracao");
            e.Property(x => x.IdUsuarioCriacao).HasColumnName("idusuariocriacao");
            e.Property(x => x.IdUsuarioAlteracao).HasColumnName("idusuarioalteracao");

            e.HasOne(x => x.Sobre)
             .WithMany(x => x.Topicos)
             .HasForeignKey(x => x.IdSobre)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}