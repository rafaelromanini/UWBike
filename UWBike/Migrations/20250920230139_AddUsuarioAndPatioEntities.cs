using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UWBike.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioAndPatioEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ANO_FABRICACAO",
                schema: "RM554637",
                table: "TB_MOTO",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ATIVO",
                schema: "RM554637",
                table: "TB_MOTO",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "COR",
                schema: "RM554637",
                table: "TB_MOTO",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DATA_ATUALIZACAO",
                schema: "RM554637",
                table: "TB_MOTO",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DATA_CRIACAO",
                schema: "RM554637",
                table: "TB_MOTO",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ID_PATIO",
                schema: "RM554637",
                table: "TB_MOTO",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TB_PATIO",
                schema: "RM554637",
                columns: table => new
                {
                    ID_PATIO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    ENDERECO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    CEP = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true),
                    CIDADE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    ESTADO = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: true),
                    TELEFONE = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: true),
                    CAPACIDADE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ATIVO = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DATA_CRIACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DATA_ATUALIZACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PATIO", x => x.ID_PATIO);
                });

            migrationBuilder.CreateTable(
                name: "TB_USUARIO",
                schema: "RM554637",
                columns: table => new
                {
                    ID_USUARIO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    SENHA = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    DATA_CRIACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DATA_ATUALIZACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_USUARIO", x => x.ID_USUARIO);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MOTO_CHASSI",
                schema: "RM554637",
                table: "TB_MOTO",
                column: "CHASSI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MOTO_PLACA",
                schema: "RM554637",
                table: "TB_MOTO",
                column: "PLACA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_MOTO_ID_PATIO",
                schema: "RM554637",
                table: "TB_MOTO",
                column: "ID_PATIO");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_EMAIL",
                schema: "RM554637",
                table: "TB_USUARIO",
                column: "EMAIL",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TB_MOTO_TB_PATIO_ID_PATIO",
                schema: "RM554637",
                table: "TB_MOTO",
                column: "ID_PATIO",
                principalSchema: "RM554637",
                principalTable: "TB_PATIO",
                principalColumn: "ID_PATIO",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TB_MOTO_TB_PATIO_ID_PATIO",
                schema: "RM554637",
                table: "TB_MOTO");

            migrationBuilder.DropTable(
                name: "TB_PATIO",
                schema: "RM554637");

            migrationBuilder.DropTable(
                name: "TB_USUARIO",
                schema: "RM554637");

            migrationBuilder.DropIndex(
                name: "IX_MOTO_CHASSI",
                schema: "RM554637",
                table: "TB_MOTO");

            migrationBuilder.DropIndex(
                name: "IX_MOTO_PLACA",
                schema: "RM554637",
                table: "TB_MOTO");

            migrationBuilder.DropIndex(
                name: "IX_TB_MOTO_ID_PATIO",
                schema: "RM554637",
                table: "TB_MOTO");

            migrationBuilder.DropColumn(
                name: "ANO_FABRICACAO",
                schema: "RM554637",
                table: "TB_MOTO");

            migrationBuilder.DropColumn(
                name: "ATIVO",
                schema: "RM554637",
                table: "TB_MOTO");

            migrationBuilder.DropColumn(
                name: "COR",
                schema: "RM554637",
                table: "TB_MOTO");

            migrationBuilder.DropColumn(
                name: "DATA_ATUALIZACAO",
                schema: "RM554637",
                table: "TB_MOTO");

            migrationBuilder.DropColumn(
                name: "DATA_CRIACAO",
                schema: "RM554637",
                table: "TB_MOTO");

            migrationBuilder.DropColumn(
                name: "ID_PATIO",
                schema: "RM554637",
                table: "TB_MOTO");
        }
    }
}
