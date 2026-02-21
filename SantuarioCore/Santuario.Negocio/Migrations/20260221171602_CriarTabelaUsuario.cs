using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Santuario.Negocio.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    login = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    senhahash = table.Column<byte[]>(type: "bytea", nullable: false),
                    senhasalt = table.Column<byte[]>(type: "bytea", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    datacriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataalteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idusuariocriacao = table.Column<int>(type: "integer", nullable: true),
                    idusuarioalteracao = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuario_login",
                table: "usuario",
                column: "login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
