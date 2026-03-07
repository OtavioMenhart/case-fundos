using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseItau.Infra.Migrations
{
    /// <inheritdoc />
    public partial class FirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TIPO_FUNDO",
                columns: table => new
                {
                    CODIGO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOME = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TIPO_FUNDO", x => x.CODIGO);
                });

            migrationBuilder.CreateTable(
                name: "FUNDO",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NOME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CNPJ = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    CODIGO_TIPO = table.Column<int>(type: "int", nullable: false),
                    PATRIMONIO = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FUNDO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FUNDO_TIPO_FUNDO_CODIGO_TIPO",
                        column: x => x.CODIGO_TIPO,
                        principalTable: "TIPO_FUNDO",
                        principalColumn: "CODIGO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FUNDO_CNPJ",
                table: "FUNDO",
                column: "CNPJ",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FUNDO_CODIGO",
                table: "FUNDO",
                column: "CODIGO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FUNDO_CODIGO_TIPO",
                table: "FUNDO",
                column: "CODIGO_TIPO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FUNDO");

            migrationBuilder.DropTable(
                name: "TIPO_FUNDO");
        }
    }
}
