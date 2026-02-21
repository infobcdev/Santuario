using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Santuario.Negocio.Migrations
{
    /// <inheritdoc />
    public partial class InitialSantuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carrossel_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    imagemurl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    datacriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataalteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idusuariocriacao = table.Column<int>(type: "integer", nullable: true),
                    idusuarioalteracao = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carrossel_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "noticia",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    imagemcapaurl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    titulo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    categoria = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    subcategoria = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    resumo = table.Column<string>(type: "text", nullable: false),
                    conteudojson = table.Column<string>(type: "jsonb", nullable: false),
                    conteudohtml = table.Column<string>(type: "text", nullable: false),
                    permitecomentarios = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    datapublicacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    datacriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataalteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idusuariocriacao = table.Column<int>(type: "integer", nullable: true),
                    idusuarioalteracao = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_noticia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sobre",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    imagemurl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    titulo1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao1 = table.Column<string>(type: "text", nullable: false),
                    titulo2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao2 = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    datacriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataalteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idusuariocriacao = table.Column<int>(type: "integer", nullable: true),
                    idusuarioalteracao = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sobre", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "noticia_comentario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idnoticia = table.Column<int>(type: "integer", nullable: false),
                    usuarioprovider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    usuarioproviderid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    usuarionome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    usuarioemail = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    datacriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataalteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idusuariocriacao = table.Column<int>(type: "integer", nullable: true),
                    idusuarioalteracao = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_noticia_comentario", x => x.id);
                    table.ForeignKey(
                        name: "FK_noticia_comentario_noticia_idnoticia",
                        column: x => x.idnoticia,
                        principalTable: "noticia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sobre_topico",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idsobre = table.Column<int>(type: "integer", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    datacriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataalteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idusuariocriacao = table.Column<int>(type: "integer", nullable: true),
                    idusuarioalteracao = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sobre_topico", x => x.id);
                    table.ForeignKey(
                        name: "FK_sobre_topico_sobre_idsobre",
                        column: x => x.idsobre,
                        principalTable: "sobre",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_noticia_slug",
                table: "noticia",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_noticia_comentario_noticia_ativo",
                table: "noticia_comentario",
                columns: new[] { "idnoticia", "ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_sobre_topico_idsobre",
                table: "sobre_topico",
                column: "idsobre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carrossel_item");

            migrationBuilder.DropTable(
                name: "noticia_comentario");

            migrationBuilder.DropTable(
                name: "sobre_topico");

            migrationBuilder.DropTable(
                name: "noticia");

            migrationBuilder.DropTable(
                name: "sobre");
        }
    }
}
