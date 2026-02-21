using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Santuario.Entidade.Entities;


namespace Santuario.Negocio.Mapeamentos
{
    public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> e)
        {
            e.ToTable("usuario");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
             .HasColumnName("id")
             .ValueGeneratedOnAdd()
             .UseIdentityByDefaultColumn();

            e.Property(x => x.Nome)
             .HasColumnName("nome")
             .HasMaxLength(120)
             .IsRequired();

            e.Property(x => x.Login)
             .HasColumnName("login")
             .HasMaxLength(80)
             .IsRequired();

            e.HasIndex(x => x.Login)
             .IsUnique();

            e.Property(x => x.SenhaHash).HasColumnType("bytea").HasColumnName("senhahash").IsRequired();
            e.Property(x => x.SenhaSalt).HasColumnType("bytea").HasColumnName("senhasalt").IsRequired();

            e.Property(x => x.Ativo)
             .HasColumnName("ativo")
             .IsRequired();

            e.Property(x => x.Tipo)
             .HasColumnName("tipo")
             .HasConversion<int>()
             .IsRequired();

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
        }
    }
}