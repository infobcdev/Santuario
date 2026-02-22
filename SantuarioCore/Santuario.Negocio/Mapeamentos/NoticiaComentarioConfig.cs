using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Santuario.Entidade.Entities;

namespace Santuario.Negocio.Mapeamentos
{
    public class NoticiaComentarioConfig : IEntityTypeConfiguration<NoticiaComentario>
    {
        public void Configure(EntityTypeBuilder<NoticiaComentario> e)
        {
            e.ToTable("noticia_comentario");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .UseIdentityByDefaultColumn();

            e.Property(x => x.IdNoticia)
                .HasColumnName("idnoticia")
                .IsRequired();

            e.Property(x => x.UsuarioProvider)
                .HasColumnName("usuarioprovider")
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("Google");

            e.Property(x => x.UsuarioProviderId)
                .HasColumnName("usuarioproviderid")
                .HasMaxLength(200)
                .IsRequired();

            e.Property(x => x.UsuarioNome)
                .HasColumnName("usuarionome")
                .HasMaxLength(200)
                .IsRequired();

            e.Property(x => x.UsuarioEmail)
                .HasColumnName("usuarioemail")
                .HasMaxLength(250);

            // texto pode ser grande => text
            e.Property(x => x.Texto)
                .HasColumnName("texto")
                .HasColumnType("text")
                .IsRequired();

            e.Property(x => x.Ativo)
                .HasColumnName("ativo")
                .HasDefaultValue(true);

            // Auditoria (base)
            e.Property(x => x.DataCriacao).HasColumnName("datacriacao");
            e.Property(x => x.DataAlteracao).HasColumnName("dataalteracao");
            e.Property(x => x.IdUsuarioCriacao).HasColumnName("idusuariocriacao");
            e.Property(x => x.IdUsuarioAlteracao).HasColumnName("idusuarioalteracao");

            e.HasOne(x => x.Noticia)
                .WithMany(x => x.Comentarios)
                .HasForeignKey(x => x.IdNoticia)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para buscar comentários rápido
            e.HasIndex(x => new { x.IdNoticia, x.Ativo })
                .HasDatabaseName("ix_noticia_comentario_noticia_ativo");

            e.HasIndex(x => new { x.IdNoticia, x.DataCriacao })
                .HasDatabaseName("ix_noticia_comentario_noticia_datacriacao");
        }
    }
}