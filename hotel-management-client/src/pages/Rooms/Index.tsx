import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { roomsApi } from "@/api/roomsApi";
import { useAuth } from "@/context/AuthContext";
import ConfirmDeleteModal from "@/components/ConfirmDeleteModal";
import Pagination from "@/components/Pagination";
import { useFlash } from "@/hooks/useFlash";
import type { RoomListItem, RoomListQuery } from "@/types/room";
import { getApiError } from "@/utils/apiError";
import { formatMoney } from "@/utils/format";
import { roomStatusLabels } from "@/utils/roomStatus";

const PAGE_SIZE = 20;
const ERROR_TEXT = "Не вдалося виконати операцію з номером.";

interface Filters {
  floor: string;
  roomType: string;
  minPrice: string;
  maxPrice: string;
}

const emptyFilters: Filters = { floor: "", roomType: "", minPrice: "", maxPrice: "" };

const toNumber = (value: string): number | undefined => {
  const n = Number(value.trim().replace(",", "."));
  return value.trim() !== "" && Number.isFinite(n) ? n : undefined;
};

export default function RoomsIndex() {
  const { hasRole } = useAuth();
  const isAdmin = hasRole("Admin");

  const [flash, setFlash] = useFlash();
  const [draft, setDraft] = useState<Filters>(emptyFilters);
  const [applied, setApplied] = useState<Filters>(emptyFilters);
  const [pageNumber, setPageNumber] = useState(1);
  const [reloadKey, setReloadKey] = useState(0);

  const [rooms, setRooms] = useState<RoomListItem[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);

  const [toDelete, setToDelete] = useState<RoomListItem | null>(null);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    const query: RoomListQuery = {
      floor: toNumber(applied.floor),
      roomType: applied.roomType.trim() || undefined,
      minPrice: toNumber(applied.minPrice),
      maxPrice: toNumber(applied.maxPrice),
      pageNumber,
      pageSize: PAGE_SIZE,
    };

    roomsApi
      .list(query)
      .then((data) => {
        if (cancelled) return;
        setRooms(data.rooms);
        setTotalCount(data.totalCount);
        setTotalPages(data.totalPages);
      })
      .catch((err) => {
        if (!cancelled) setFlash({ type: "danger", text: getApiError(err, ERROR_TEXT) });
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [applied, pageNumber, reloadKey, setFlash]);

  const handleFilter = (e: React.FormEvent) => {
    e.preventDefault();
    setApplied(draft);
    setPageNumber(1);
  };

  const handleClear = () => {
    setDraft(emptyFilters);
    setApplied(emptyFilters);
    setPageNumber(1);
  };

  const confirmDelete = async () => {
    if (!toDelete) return;
    setDeleting(true);
    try {
      await roomsApi.remove(toDelete.id);
      setFlash({ type: "success", text: "Номер видалено." });
      setReloadKey((k) => k + 1);
    } catch (err) {
      setFlash({ type: "danger", text: getApiError(err, ERROR_TEXT) });
    } finally {
      setDeleting(false);
      setToDelete(null);
    }
  };

  return (
    <div>
      <div className="page-heading rooms-page-heading">
        <div className="rooms-heading-content">
          <h1>Список номерів</h1>
          <p className="page-subtitle">Список номерів, редагування та фільтрація.</p>
        </div>
      </div>

      {flash && <div className={`alert app-alert alert-${flash.type}`}>{flash.text}</div>}

      <section className="content-card form-card mb-4 rooms-filter-card">
        <form onSubmit={handleFilter}>
          <div className="room-filter-row">
            <div className="form-field form-field-inline">
              <label htmlFor="floor" className="form-label">Поверх</label>
              <input
                id="floor"
                type="text"
                className="form-control"
                value={draft.floor}
                onChange={(e) => setDraft({ ...draft, floor: e.target.value })}
              />
            </div>
            <div className="form-field form-field-inline">
              <label htmlFor="roomType" className="form-label">Тип номера</label>
              <input
                id="roomType"
                type="text"
                className="form-control"
                value={draft.roomType}
                onChange={(e) => setDraft({ ...draft, roomType: e.target.value })}
              />
            </div>
            <div className="form-field form-field-inline">
              <label htmlFor="minPrice" className="form-label">Ціна від</label>
              <input
                id="minPrice"
                type="number"
                step="0.01"
                className="form-control"
                value={draft.minPrice}
                onChange={(e) => setDraft({ ...draft, minPrice: e.target.value })}
              />
            </div>
            <div className="form-field form-field-inline">
              <label htmlFor="maxPrice" className="form-label">Ціна до</label>
              <input
                id="maxPrice"
                type="number"
                step="0.01"
                className="form-control"
                value={draft.maxPrice}
                onChange={(e) => setDraft({ ...draft, maxPrice: e.target.value })}
              />
            </div>

            <div className="room-filter-buttons">
              <button type="submit" className="btn btn-teal">Фільтрувати</button>
              <button type="button" className="btn btn-cancel" onClick={handleClear}>
                Очистити
              </button>
            </div>

            <Link to="/rooms/new" className="btn btn-teal rooms-add-btn">
              Додати кімнату
            </Link>
          </div>
        </form>
      </section>

      <section className="content-card table-card mb-4">
        <div className="card-heading">
          <div>
            <h2>Список кімнат</h2>
            <p>{totalCount} номерів</p>
          </div>
        </div>

        <div className="table-responsive">
          <table className="hotel-table">
            <thead className="text-center">
              <tr>
                <th>Номер</th>
                <th>Поверх</th>
                <th>Тип</th>
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
                    <td>{roomStatusLabels[room.operationalStatus]}</td>
                    <td>
                      <div className="d-flex align-items-center justify-content-center gap-2">
                        <Link to={`/rooms/${room.id}/edit`} className="btn btn-teal">
                          Редагувати
                        </Link>
                        {isAdmin && (
                          <button
                            type="button"
                            className="btn btn-danger"
                            onClick={() => setToDelete(room)}
                          >
                            Видалити
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>

        <Pagination
          pageNumber={pageNumber}
          totalPages={totalPages}
          ariaLabel="Навігація сторінками номерів"
          onChange={setPageNumber}
        />
      </section>

      {toDelete && (
        <ConfirmDeleteModal
          title="Видалити номер?"
          rows={[
            { label: "Номер", value: toDelete.roomNumber },
            { label: "Тип номера", value: toDelete.type },
            { label: "Ціна за ніч (грн)", value: formatMoney(toDelete.pricePerDay) },
            { label: "Місткість", value: toDelete.capacity },
          ]}
          warning="Після видалення номер неможливо буде відновити."
          confirmLabel="Видалити номер"
          busy={deleting}
          onConfirm={confirmDelete}
          onClose={() => setToDelete(null)}
        />
      )}
    </div>
  );
}