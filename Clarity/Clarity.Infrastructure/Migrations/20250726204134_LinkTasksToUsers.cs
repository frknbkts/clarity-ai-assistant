﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clarity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkTasksToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TaskItems",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_UserId",
                table: "TaskItems",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_AspNetUsers_UserId",
                table: "TaskItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_AspNetUsers_UserId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_UserId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TaskItems");
        }
    }
}
