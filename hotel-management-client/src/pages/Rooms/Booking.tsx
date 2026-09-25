import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { roomsApi } from "@/api/roomsApi";
import type { RoomPeriodStatusDto, RoomPeriodStatusQuery } from "@/types/room";

const today = () => new Date().toISOString().slice(0, 10);
const tomorrow = () => {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  return d.toISOString().slice(0, 10);
};

export default function RoomBooking() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [query, setQuery] = useState<RoomPeriodStatusQuery>({
    startDate: today(),
    endDate: tomorrow(),
    pageNumber: 1,
    pageSize: 30,
  });
  const [rooms, setRooms] = useState<RoomPeriodStatusDto[]>([]);
  const [floors, setFloors] = useState<number[]>([]);
  const [roomTypes, setRoomTypes] = useState<string[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    roomsApi
      .periodStatus(query)
      .then((data) => {
        setRooms(data.rooms);
        setFloors(data.floors);
        setRoomTypes(data.roomTypes);
        setTotalPages(data.totalPages);
      })
      .finally(() => setLoading(false));
  }, [query]);

  const updateFilter = (patch: Partial<RoomPeriodStatusQuery>) =>
    setQuery((prev) => ({ ...prev, ...patch, pageNumber: 1 }));

  const goToBooking = (roomId: number) => {
    navigate(
      `/reservations/new?roomId=${roomId}&checkIn=${query.startDate}&checkOut=${query.endDate}`
    );
  };

  return (
    <div>
      <h1>Статус номерів на період{id ? ` (номер #${id})` : ""}</h1>

      <form className="filters" onSubmit={(e) => e.preventDefault()}>
        <div className="form-group">
          <label>Дата початку</label>
          <input
            type="date"
            value={query.startDate}
            onChange={(e) => updateFilter({ startDate: e.target.value })}
          />
        </div>
        <div className="form-group">
          <label>Дата закінчення</label>
          <input
            type="date"
            value={query.endDate}
            onChange={(e) => updateFilter({ endDate: e.target.value })}
          />
        </div>
        <div className="form-group">
          <label>Кількість гостей</label>
          <input
            type="number"
            min={1}
            max={20}
            value={query.guestsCount ?? ""}
            onChange={(e) =>
              updateFilter({ guestsCount: e.target.value ? Number(e.target.value) : undefined })
            }
          />
        </div>

        <select
          value={query.floor ?? ""}
          onChange={(e) =>
            updateFilter({ floor: e.target.value ? Number(e.target.value) : undefined })
          }
        >
          <option value="">Всі поверхи</option>
          {floors.map((f) => (
            <option key={f} value={f}>
              Поверх {f}
            </option>
          ))}
        </select>

        <select
          value={query.roomType ?? ""}
          onChange={(e) => updateFilter({ roomType: e.target.value || undefined })}
        >
          <option value="">Всі типи</option>
          {roomTypes.map((t) => (
            <option key={t} value={t}>
              {t}
            </option>
          ))}
        </select>
      </form>

      {loading ? (
        <p>Завантаження...</p>
      ) : (
        <table className="table">
          <thead>
            <tr>
              <th>№</th>
              <th>Поверх</th>
              <th>Тип</th>
              <th>Ціна/добу</th>
              <th>Місткість</th>
              <th>Доступність</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {rooms.map((room) => (
              <tr key={room.id}>
                <td>{room.roomNumber}</td>
                <td>{room.floor}</td>
                <td>{room.type}</td>
                <td>{room.pricePerDay} ₴</td>
                <td>{room.capacity}</td>
                <td>
                  <span className={room.isAvailable ? "badge-success" : "badge-danger"}>
                    {room.isAvailable ? "Вільний" : "Зайнятий"}
                  </span>
                </td>
                <td>
                  {room.isAvailable && (
                    <button onClick={() => goToBooking(room.id)}>Забронювати</button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <div className="pagination">
        {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
          <button
            key={page}
            className={page === query.pageNumber ? "active" : ""}
            onClick={() => setQuery((prev) => ({ ...prev, pageNumber: page }))}
          >
            {page}
          </button>
        ))}
      </div>
    </div>
  );
}