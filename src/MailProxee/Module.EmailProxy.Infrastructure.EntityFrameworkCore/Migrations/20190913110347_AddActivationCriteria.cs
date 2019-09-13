using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Module.EmailProxy.Infrastructure.EntityFrameworkCore.Migrations
{
    public partial class AddActivationCriteria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivationCriteria_ActivationCode",
                table: "Aliases",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationCriteria_Creation",
                table: "Aliases",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ActivationCriteria_IsActivated",
                table: "Aliases",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ActivationCriteria_IsSent",
                table: "Aliases",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivationCriteria_ActivationCode",
                table: "Aliases");

            migrationBuilder.DropColumn(
                name: "ActivationCriteria_Creation",
                table: "Aliases");

            migrationBuilder.DropColumn(
                name: "ActivationCriteria_IsActivated",
                table: "Aliases");

            migrationBuilder.DropColumn(
                name: "ActivationCriteria_IsSent",
                table: "Aliases");
        }
    }
}
