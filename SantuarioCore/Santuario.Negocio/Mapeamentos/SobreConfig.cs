using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Santuario.Entidade.Entities;

namespace Santuario.Negocio.Mapeamentos
{
    public class SobreConfig : IEntityTypeConfiguration<Sobre>
    {
        public void Configure(EntityTypeBuilder<Sobre> e)
        {
            e.ToTable("sobre");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
             .HasColumnName("id")
             .ValueGeneratedOnAdd()
             .UseIdentityByDefaultColumn();

            e.Property(x => x.ImagemUrl)
             .HasColumnName("imagemurl")
             .HasMaxLength(500);

            e.Property(x => x.Titulo1)
             .HasColumnName("titulo1")
             .HasMaxLength(200);

            e.Property(x => x.Descricao1)
             .HasColumnName("descricao1");

            e.Property(x => x.Titulo2)
             .HasColumnName("titulo2")
             .HasMaxLength(200);

            e.Property(x => x.Descricao2)
             .HasColumnName("descricao2");

            e.Property(x => x.Ativo)
             .HasColumnName("ativo");

            // ===============================
            // Auditoria (classe base)
            // ===============================

            e.Property(x => x.DataCriacao)
             .HasColumnName("datacriacao");

            e.Property(x => x.DataAlteracao)
             .HasColumnName("dataalteracao");

            e.Property(x => x.IdUsuarioCriacao)
             .HasColumnName("idusuariocriacao");

            e.Property(x => x.IdUsuarioAlteracao)
             .HasColumnName("idusuarioalteracao");

            // ===============================
            // Relacionamento Sobre -> Topicos
            // ===============================

            e.HasMany(x => x.Topicos)
             .WithOne(x => x.Sobre)
             .HasForeignKey(x => x.IdSobre)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}