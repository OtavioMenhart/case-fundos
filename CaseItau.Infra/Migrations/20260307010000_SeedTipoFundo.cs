using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseItau.Infra.Migrations
{
    /// <inheritdoc />
    public partial class SeedTipoFundo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TIPO_FUNDO",
                columns: new[] { "CODIGO", "NOME" },
                values: new object[,]
                {
                    { 1, "RENDA FIXA" },
                    { 2, "ACOES" },
                    { 3, "MULTI MERCADO" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TIPO_FUNDO",
                keyColumn: "CODIGO",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TIPO_FUNDO",
                keyColumn: "CODIGO",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TIPO_FUNDO",
                keyColumn: "CODIGO",
                keyValue: 3);
        }
    }
}
