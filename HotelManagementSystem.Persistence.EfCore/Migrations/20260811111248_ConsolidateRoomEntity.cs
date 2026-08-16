using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagementSystem.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateRoomEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Rooms",
                newName: "PricePerDay");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Rooms",
                newName: "RoomNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_Number",
                table: "Rooms",
                newName: "IX_Rooms_RoomNumber");

            migrationBuilder.AddColumn<string>(
                name: "BookedDates",
                table: "Rooms",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Rooms",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Rooms" AS r
                SET "Type" = rt."Name"
                FROM "RoomTypes" AS rt
                WHERE r."RoomTypeId" = rt."Id";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_RoomTypes_RoomTypeId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_RoomTypeId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "RoomTypeId",
                table: "Rooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookedDates",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "RoomNumber",
                table: "Rooms",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "PricePerDay",
                table: "Rooms",
                newName: "Price");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_RoomNumber",
                table: "Rooms",
                newName: "IX_Rooms_Number");

            migrationBuilder.AddColumn<int>(
                name: "RoomTypeId",
                table: "Rooms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomTypeId",
                table: "Rooms",
                column: "RoomTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_RoomTypes_RoomTypeId",
                table: "Rooms",
                column: "RoomTypeId",
                principalTable: "RoomTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
