import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { reservationsApi } from "@/api/reservationsApi";
import { ReservationStatus, type ReservationStatus as ReservationStatusType } from "@/types/enums";
import type { ReservationDto, ReservationsIndexQuery } from "@/types/reservation";

const statusLabels: Record<ReservationStatusType, string> = {
  [ReservationStatus.Pending]: "Очікує",
  [ReservationStatus.Confirmed]: "Підтверджено",
  [ReservationStatus.CheckedIn]: "Заселено",
  [ReservationStatus.CheckedOut]: "Виселено",
  [ReservationStatus.Cancelled]: "Скасовано",
};

export default function ReservationsIndex() {
  const [reservations, setReservations] = useState<ReservationDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [query, setQuery] = useState<ReservationsIndexQuery>({ pageNumber: 1, pageSize: 30 });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    reservationsApi
      .list(query)
      .then((data) => {
        setReservations(data.reservations);
        setTotalPages(data.totalPages);
      })
      .finally(() => setLoading(false));
  }, [query]);

  const updateFilter = (patch: Partial<ReservationsIndexQuery>) =>
    setQuery((prev) => ({ ...prev, ...patch, pageNumber: 1 }));

  const changeStatus = async (id: number, status: ReservationStatusType) => {
    await reservationsApi.changeStatus({ id, status });
    setQuery((prev) => ({ ...prev }));
  };

  return (
    <div>
      <div className="page-header">
        <h1>Бронювання</h1>
        <Link to="/reservations/new" className="btn btn-primary">
          Нове бронювання
        </Link>
      </div>

      <form className="filters" onSubmit={(e) => e.preventDefault()}>
        <select
          value={query.status ?? ""}
          onChange={(e) =>
            updateFilter({ status: (e.target.value || undefined) as ReservationStatusType | undefined })
          }
        >
          <option value="">Всі статуси</option>
          {Object.entries(statusLabels).map(([value, label]) => (
            <option key={value} value={value}>
              {label}
            </option>
          ))}
        </select>

        <input
          type="text"
          placeholder="Пошук гостя"
          value={query.guestSearch ?? ""}
          onChange={(e) => updateFilter({ guestSearch: e.target.value || undefined })}
        />

        <input
          type="text"
          placeholder="Номер кімнати"
          value={query.roomNumber ?? ""}
          onChange={(e) => updateFilter({ roomNumber: e.target.value || undefined })}
        />

        <div className="form-group">
          <label>Заселення з</label>
          <input
            type="date"
            value={query.checkInFrom ?? ""}
            onChange={(e) => updateFilter({ checkInFrom: e.target.value || undefined })}
          />
        </div>
        <div className="form-group">
          <label>Заселення до</label>
          <input
            type="date"
            value={query.checkInTo ?? ""}
            onChange={(e) => updateFilter({ checkInTo: e.target.value || undefined })}
          />
        </div>
      </form>

      {loading ? (
        <p>Завантаження...</p>
      ) : (
        <table className="table">
          <thead>
            <tr>
              <th>Гість</th>
              <th>Номер</th>
              <th>Заселення</th>
              <th>Виселення</th>
              <th>Гостей</th>
              <th>Сума</th>
              <th>Статус</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {reservations.map((res) => (
              <tr key={res.id}>
                <td>
                  {res.guestFullName}
                  <div className="text-muted">{res.guestEmail}</div>
                </td>
                <td>
                  {res.roomNumber} ({res.roomType})
                </td>
                <td>{res.checkIn}</td>
                <td>{res.checkOut}</td>
                <td>{res.guestsCount}</td>
                <td>{res.totalPrice} ₴</td>
                <td>
                  <select
                    value={res.status}
                    onChange={(e) => changeStatus(res.id, e.target.value as ReservationStatusType)}
                  >
                    {Object.entries(statusLabels).map(([value, label]) => (
                      <option key={value} value={value}>
                        {label}
                      </option>
                    ))}
                  </select>
                </td>
                <td className="actions">
                  <Link to={`/reservations/${res.id}/edit`}>Редагувати</Link>
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