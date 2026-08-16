using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagementSystem.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class PreventUnavailableRoomReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION "PreventUnavailableRoomReservation"()
                RETURNS trigger AS $$
                DECLARE room_status text;
                BEGIN
                    SELECT "Status" INTO room_status
                    FROM "Rooms"
                    WHERE "Id" = NEW."RoomId";

                    IF room_status IS NULL THEN
                        RAISE EXCEPTION 'Номер не знайдено.';
                    END IF;

                    IF room_status <> 'Available' THEN
                        RAISE EXCEPTION 'Неможливо забронювати номер, який не є доступним.';
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER "TR_Reservations_PreventUnavailableRoomReservation"
                BEFORE INSERT OR UPDATE OF "RoomId" ON "Reservations"
                FOR EACH ROW
                EXECUTE FUNCTION "PreventUnavailableRoomReservation"();
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TR_Reservations_PreventUnavailableRoomReservation" ON "Reservations";
                """);

            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS "PreventUnavailableRoomReservation"();
                """);

        }
    }
}
