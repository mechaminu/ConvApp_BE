using Microsoft.EntityFrameworkCore.Migrations;

namespace ConvAppServer.Migrations
{
    public partial class _2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostingNode_Postings_PostingId",
                table: "PostingNode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostingNode",
                table: "PostingNode");

            migrationBuilder.DropIndex(
                name: "IX_PostingNode_PostingId",
                table: "PostingNode");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PostingNode");

            migrationBuilder.AlterColumn<int>(
                name: "PostingId",
                table: "PostingNode",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostingNode",
                table: "PostingNode",
                columns: new[] { "PostingId", "OrderIndex" });

            migrationBuilder.AddForeignKey(
                name: "FK_PostingNode_Postings_PostingId",
                table: "PostingNode",
                column: "PostingId",
                principalTable: "Postings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostingNode_Postings_PostingId",
                table: "PostingNode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostingNode",
                table: "PostingNode");

            migrationBuilder.AlterColumn<int>(
                name: "PostingId",
                table: "PostingNode",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PostingNode",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostingNode",
                table: "PostingNode",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PostingNode_PostingId",
                table: "PostingNode",
                column: "PostingId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostingNode_Postings_PostingId",
                table: "PostingNode",
                column: "PostingId",
                principalTable: "Postings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
