import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { roomsApi } from "@/api/roomsApi";
import { useAuth } from "@/context/AuthContext";
import type { RoomListItem, RoomListQuery } from "@/types/room";

export default function RoomsIndex() {
  const { hasRole } = useAuth();
  const [rooms, setRooms] = useState<RoomListItem[]>([]);
  const [floors, setFloors] = useState<number[]>([]);
  const [roomTypes, setRoomTypes] = useState<string[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [query, setQuery] = useState<RoomListQuery>({ pageNumber: 1, pageSize: 30 });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    roomsApi
      .list(query)
      .then((data) => {
        setRooms(data.rooms);
        setFloors(data.floors);
        setRoomTypes(data.roomTypes);
        setTotalPages(data.totalPages);
      })
      .finally(() => setLoading(false));
  }, [query]);

  const canManage = hasRole("Manager", "Admin");

  const updateFilter = (patch: Partial<RoomListQuery>) =>
    setQuery((prev) => ({ ...prev, ...patch, pageNumber: 1 }));

  return (
    <div>
      <div className="page-header">
        <h1>Номери</h1>
        {canManage && (
          <Link to="/rooms/new" className="btn btn-primary">
            Додати номер
          </Link>
        )}
      </div>

      <form className="filters" onSubmit={(e) => e.preventDefault()}>
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

        <input
          type="number"
          placeholder="Ціна від"
          value={query.minPrice ?? ""}
          onChange={(e) =>
            updateFilter({ minPrice: e.target.value ? Number(e.target.value) : undefined })
          }
        />
        <input
          type="number"
          placeholder="Ціна до"
          value={query.maxPrice ?? ""}
          onChange={(e) =>
            updateFilter({ maxPrice: e.target.value ? Number(e.target.value) : undefined })
          }
        />
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
              <th>Статус</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {rooms.map((room) => (
              <tr key={room.id}>
                <td>{room.roomNumber}</td>
                <td>{room.floor}</td>
                <td>{room.displayType}</td>
                <td>{room.pricePerDay} ₴</td>
                <td>{room.capacity}</td>
                <td>{room.displayStatus}</td>
                <td className="actions">
                  <Link to={`/rooms/${room.id}/booking`}>Бронювання</Link>
                  {canManage && <Link to={`/rooms/${room.id}/edit`}>Редагувати</Link>}
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