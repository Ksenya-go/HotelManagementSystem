import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { roomsApi } from "@/api/roomsApi";
import Pagination from "@/components/Pagination";
import type { RoomPeriodStatusDto } from "@/types/room";
import { addDays, todayInput } from "@/utils/date";
import { formatMoney } from "@/utils/format";
import { roomStatusLabels } from "@/utils/roomStatus";

const PAGE_SIZE = 30;
const DATE_RANGE_ERROR = "Дата закінчення має бути пізнішою за дату початку.";
const GENERIC_ERROR = "Не вдалося виконати операцію з номером.";

interface Draft {
  startDate: string;
  endDate: string;
  guestsCount: string;
}

export default function RoomBooking() {
  const [searchParams] = useSearchParams();

  const [draft, setDraft] = useState<Draft>(() => {
    const startDate = searchParams.get("startDate") || todayInput();
    const endDate = searchParams.get("endDate") || addDays(startDate, 1);
    const guestsCount = searchParams.get("guestsCount") || "1";
    return { startDate, endDate, guestsCount };
  });
  const [applied, setApplied] = useState<Draft>(draft);
  const [pageNumber, setPageNumber] = useState(1);

  const [rooms, setRooms] = useState<RoomPeriodStatusDto[]>([]);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (applied.endDate <= applied.startDate) {
      setRooms([]);
      setTotalPages(0);
      setError(DATE_RANGE_ERROR);
      setLoading(false);
      return;
    }

    let cancelled = false;
    setLoading(true);
    setError(null);

    roomsApi
      .periodStatus({
        startDate: applied.startDate,
        endDate: applied.endDate,
        guestsCount: Math.max(1, Number(applied.guestsCount) || 1),
        pageNumber,
        pageSize: PAGE_SIZE,
      })
      .then((data) => {
        if (cancelled) return;
        setRooms(data.rooms.filter((room) => room.canBook));
        setTotalPages(data.totalPages);
      })
      .catch(() => {
        if (!cancelled) {
          setRooms([]);
          setError(GENERIC_ERROR);
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [applied, pageNumber]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setApplied(draft);
    setPageNumber(1);
  };

  const guests = Math.max(1, Number(applied.guestsCount) || 1);

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Бронювання номерів</h1>
          <p className="page-subtitle">
            Оберіть дати та знайдіть доступні номери для бронювання.
          </p>
        </div>
      </div>

      <section className="content-card form-card mb-4">
        <form onSubmit={handleSubmit}>
          {error && <div className="validation-summary">{error}</div>}
          <div className="row g-3 align-items-end">
            <div className="col-12 col-md-3">
              <label htmlFor="startDate" className="form-label">Дата початку</label>
              <input
                id="startDate"
                type="date"
                className="form-control"
                required
                value={draft.startDate}
                onChange={(e) => setDraft({ ...draft, startDate: e.target.value })}
              />
            </div>
            <div className="col-12 col-md-3">
              <label htmlFor="endDate" className="form-label">Дата закінчення</label>
              <input
                id="endDate"
                type="date"
                className="form-control"
                required
                value={draft.endDate}
                onChange={(e) => setDraft({ ...draft, endDate: e.target.value })}
              />
            </div>
            <div className="col-12 col-md-3">
              <label htmlFor="guestsCount" className="form-label">Кількість гостей</label>
              <input
                id="guestsCount"
                type="number"
                min={1}
                max={20}
                className="form-control"
                placeholder="Необов’язково"
                value={draft.guestsCount}
                onChange={(e) => setDraft({ ...draft, guestsCount: e.target.value })}
              />
            </div>
            <div className="col-12 col-md-3">
              <button type="submit" className="btn btn-teal w-100">
                Показати доступні номери
              </button>
            </div>
          </div>
        </form>
      </section>

      <section className="content-card table-card mb-4">
        <div className="card-heading">
          <div>
            <h2>Доступні номери</h2>
            <p>{rooms.length} номерів</p>
          </div>
        </div>

        <div className="table-responsive">
          <table className="hotel-table">
            <thead className="text-center">
              <tr>
                <th>Номер кімнати</th>
                <th>Поверх</th>
                <th>Тип номера</th>
                <th>Опис</th>
                <th>Ціна за ніч (грн)</th>
                <th>Місткість</th>
                <th>Кількість кімнат</th>
                <th>Статус кімнати</th>
                <th>Дії</th>
              </tr>
            </thead>
            <tbody className="text-center">
              {loading && (
                <tr>
                  <td colSpan={9}>Завантаження...</td>
                </tr>
              )}
              {!loading &&
                rooms.map((room) => (
                  <tr key={room.id}>
                    <td><strong>{room.roomNumber}</strong></td>
                    <td>{room.floor}</td>
                    <td>{room.type}</td>
                    <td>{room.description?.trim() ? room.description : "—"}</td>
                    <td>{formatMoney(room.pricePerDay)}</td>
                    <td>{room.capacity}</td>
                    <td>{room.roomCount}</td>
                    <td>{roomStatusLabels[room.operationalStatus] ?? room.operationalStatus}</td>
                    <td>
                      <Link
                        to={`/reservations/new?roomId=${room.id}&checkIn=${applied.startDate}&checkOut=${applied.endDate}&guestsCount=${guests}`}
                        className="btn btn-teal"
                      >
                        Забронювати
                      </Link>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>

        <Pagination
          pageNumber={pageNumber}
          totalPages={totalPages}
          ariaLabel="Навігація сторінками доступних номерів"
          onChange={setPageNumber}
        />
      </section>
    </div>
  );
}