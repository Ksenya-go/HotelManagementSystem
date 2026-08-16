using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagementSystem.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class SplitRoomOperationalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Rooms"
                SET "Status" = CASE "Status"
                    WHEN 'Available' THEN 'Clean'
                    WHEN 'Occupied' THEN 'Clean'
                    WHEN 'Maintenance' THEN 'InMaintenance'
                    WHEN 'Cleaning' THEN 'Cleaning'
                    ELSE 'Clean'
                END;
                """);

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

                    IF room_status <> 'Clean' THEN
                        RAISE EXCEPTION 'Неможливо забронювати номер, який не є чистим або перебуває на обслуговуванні.';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM "Reservations"
                        WHERE "RoomId" = NEW."RoomId"
                            AND "Id" <> COALESCE(NEW."Id", 0)
                            AND "Status" IN ('Confirmed', 'CheckedIn')
                            AND "CheckIn" < NEW."CheckOut"
                            AND NEW."CheckIn" < "CheckOut"
                    ) THEN
                        RAISE EXCEPTION 'Номер уже зайнятий на вибрані дати.';
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                UPDATE "Rooms"
                SET "Status" = CASE "Status"
                    WHEN 'Clean' THEN 'Available'
                    WHEN 'InMaintenance' THEN 'Maintenance'
                    WHEN 'Cleaning' THEN 'Cleaning'
                    ELSE 'Available'
                END;
                """);

        }
    }
}
