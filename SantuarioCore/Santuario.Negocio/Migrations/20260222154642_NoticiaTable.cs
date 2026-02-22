using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Santuario.Negocio.Migrations
{
    /// <inheritdoc />
    public partial class NoticiaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "usuarioprovider",
                table: "noticia_comentario",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Google",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "ativo",
                table: "noticia_comentario",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "noticia",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "resumo",
                table: "noticia",
                type: "character varying(600)",
                maxLength: 600,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "permitecomentarios",
                table: "noticia",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.CreateIndex(
                name: "ix_noticia_comentario_noticia_datacriacao",
                table: "noticia_comentario",
                columns: new[] { "idnoticia", "datacriacao" });

            migrationBuilder.CreateIndex(
                name: "ix_noticia_categoria_status",
                table: "noticia",
                columns: new[] { "categoria", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_noticia_status_datapublicacao",
                table: "noticia",
                columns: new[] { "status", "datapublicacao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_noticia_comentario_noticia_datacriacao",
                table: "noticia_comentario");

            migrationBuilder.DropIndex(
                name: "ix_noticia_categoria_status",
                table: "noticia");

            migrationBuilder.DropIndex(
                name: "ix_noticia_status_datapublicacao",
                table: "noticia");

            migrationBuilder.AlterColumn<string>(
                name: "usuarioprovider",
                table: "noticia_comentario",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Google");

            migrationBuilder.AlterColumn<bool>(
                name: "ativo",
                table: "noticia_comentario",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "noticia",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "resumo",
                table: "noticia",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(600)",
                oldMaxLength: 600);

            migrationBuilder.AlterColumn<bool>(
                name: "permitecomentarios",
                table: "noticia",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }
    }
}
