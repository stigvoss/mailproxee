using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Module.EmailProxy.Infrastructure.EntityFrameworkCore.Migrations
{
    public partial class AddMailDomain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DomainId",
                table: "Aliases",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aliases_DomainId",
                table: "Aliases",
                column: "DomainId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aliases_Domains_DomainId",
                table: "Aliases",
                column: "DomainId",
                principalTable: "Domains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aliases_Domains_DomainId",
                table: "Aliases");

            migrationBuilder.DropTable(
                name: "Domains");

            migrationBuilder.DropIndex(
                name: "IX_Aliases_DomainId",
                table: "Aliases");

            migrationBuilder.DropColumn(
                name: "DomainId",
                table: "Aliases");
        }
    }
}
