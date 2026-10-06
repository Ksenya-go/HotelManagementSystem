import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { reservationsApi } from "@/api/reservationsApi";
import { useAuth } from "@/context/AuthContext";
import ConfirmDeleteModal from "@/components/ConfirmDeleteModal";
import Pagination from "@/components/Pagination";
import { useFlash } from "@/hooks/useFlash";
import type { ReservationStatus } from "@/types/enums";
import type { ReservationDto, ReservationsIndexQuery } from "@/types/reservation";
import { getApiError } from "@/utils/apiError";
import { formatDate } from "@/utils/format";
import { reservationStatuses, reservationStatusLabels } from "@/utils/reservationStatus";

const PAGE_SIZE = 20;
const ERROR_TEXT = "Не вдалося виконати операцію з бронюванням.";

interface Filters {
  status: string;
  guestSearch: string;
  roomNumber: string;
}

const emptyFilters: Filters = { status: "", guestSearch: "", roomNumber: "" };

function StatusForm({
  reservation,
  onSave,
}: {
  reservation: ReservationDto;
  onSave: (status: ReservationStatus) => Promise<void>;
}) {
  const [status, setStatus] = useState<ReservationStatus>(reservation.status);
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await onSave(status);
    } finally {
      setSaving(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="reservation-status-form">
      <select
        className="form-select form-select-sm reservation-status-select"
        value={status}
        onChange={(e) => setStatus(e.target.value as ReservationStatus)}
      >
        {reservationStatuses.map((s) => (
          <option key={s} value={s}>
            {reservationStatusLabels[s]}
          </option>
        ))}
      </select>
      <button type="submit" className="btn btn-teal" disabled={saving}>
        Зберегти зміни
      </button>
    </form>
  );
}

export default function ReservationsIndex() {
  const { hasRole } = useAuth();
  const isAdmin = hasRole("Admin");

  const [flash, setFlash] = useFlash();
  const [draft, setDraft] = useState<Filters>(emptyFilters);
  const [applied, setApplied] = useState<Filters>(emptyFilters);
  const [pageNumber, setPageNumber] = useState(1);
  const [reloadKey, setReloadKey] = useState(0);

  const [reservations, setReservations] = useState<ReservationDto[]>([]);
  const [checkInTime, setCheckInTime] = useState("");
  const [checkOutTime, setCheckOutTime] = useState("");
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);

  const [toDelete, setToDelete] = useState<ReservationDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    const query: ReservationsIndexQuery = {
      status: (applied.status || undefined) as ReservationStatus | undefined,
      guestSearch: applied.guestSearch.trim() || undefined,
      roomNumber: applied.roomNumber.trim() || undefined,
      pageNumber,
      pageSize: PAGE_SIZE,
    };

    reservationsApi
      .list(query)
      .then((data) => {
        if (cancelled) return;
        setReservations(data.reservations);
        setCheckInTime(data.checkInTime);
        setCheckOutTime(data.checkOutTime);
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

  const changeStatus = async (reservation: ReservationDto, status: ReservationStatus) => {
    try {
      await reservationsApi.changeStatus({ id: reservation.id, status });
      setFlash({ type: "success", text: "Статус бронювання оновлено." });
      setReloadKey((k) => k + 1);
    } catch (err) {
      setFlash({ type: "danger", text: getApiError(err, ERROR_TEXT) });
    }
  };

  const confirmDelete = async () => {
    if (!toDelete) return;
    setDeleting(true);
    try {
      await reservationsApi.remove(toDelete.id);
      setFlash({ type: "success", text: "Бронювання видалено." });
      setReloadKey((k) => k + 1);
    } catch (err) {
      setFlash({ type: "danger", text: getApiError(err, ERROR_TEXT) });
    } finally {
      setDeleting(false);
      setToDelete(null);
    }
  };

  const orDash = (value: string | undefined) => (value?.trim() ? value : "—");
  const orNotSpecified = (value: string | undefined) => (value?.trim() ? value : "Не вказано");

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Список бронювань</h1>
          <p className="page-subtitle">Перегляд та редагування бронювань.</p>
        </div>
      </div>

      {flash && <div className={`alert app-alert alert-${flash.type}`}>{flash.text}</div>}

      <section className="content-card reservation-filter-card">
        <form onSubmit={handleFilter}>
          <div className="reservation-filter-row">
            <div className="form-field reservation-status-field">
              <label htmlFor="status" className="form-label">Статус</label>
              <select
                id="status"
                className="form-select"
                value={draft.status}
                onChange={(e) => setDraft({ ...draft, status: e.target.value })}
              >
                <option value="">Усі статуси</option>
                {reservationStatuses.map((s) => (
                  <option key={s} value={s}>
                    {reservationStatusLabels[s]}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field reservation-guest-field">
              <label htmlFor="guestSearch" className="form-label">Гість</label>
              <input
                id="guestSearch"
                className="form-control"
                placeholder="Ім’я або прізвище"
                value={draft.guestSearch}
                onChange={(e) => setDraft({ ...draft, guestSearch: e.target.value })}
              />
            </div>

            <div className="form-field reservation-room-field">
              <label htmlFor="roomNumber" className="form-label">Номер</label>
              <input
                id="roomNumber"
                className="form-control"
                value={draft.roomNumber}
                onChange={(e) => setDraft({ ...draft, roomNumber: e.target.value })}
              />
            </div>

            <div className="reservation-filter-actions">
              <button type="submit" className="btn btn-teal">Фільтрувати</button>
              <button type="button" className="btn btn-cancel" onClick={handleClear}>
                Очистити
              </button>
            </div>
          </div>
        </form>
      </section>

      <section className="content-card table-card reservations-table-card">
        <div className="reservations-table-heading">
          <div>
            <h2>Список бронювань</h2>
            <span className="table-count">{totalCount} бронювань</span>
          </div>
        </div>

        <div className="table-responsive">
          <table className="hotel-table reservations-table">
            <thead className="text-center">
              <tr>
                <th>Гість</th>
                <th>Електронна пошта</th>
                <th>Номер</th>
                <th>Заселення</th>
                <th>Виселення</th>
                <th>Статус</th>
                <th className="reservation-actions-header">Дії</th>
              </tr>
            </thead>
            <tbody className="text-center">
              {loading && (
                <tr>
                  <td colSpan={7}>Завантаження...</td>
                </tr>
              )}
              {!loading &&
                reservations.map((r) => (
                  <tr key={r.id}>
                    <td>
                      <span className="reservation-guest-name">{orNotSpecified(r.guestFullName)}</span>
                    </td>
                    <td>
                      <span className="reservation-email">{orNotSpecified(r.guestEmail)}</span>
                    </td>
                    <td>
                      <span className="reservation-room">{orDash(r.roomNumber)}</span>
                    </td>
                    <td className="reservation-date">
                      <span>{formatDate(r.checkIn)}</span> <small>{checkInTime}</small>
                    </td>
                    <td className="reservation-date">
                      <span>{formatDate(r.checkOut)}</span> <small>{checkOutTime}</small>
                    </td>
                    <td className="reservation-status-cell">
                      <StatusForm reservation={r} onSave={(status) => changeStatus(r, status)} />
                    </td>
                    <td>
                      <div className="d-flex align-items-center justify-content-center gap-2 reservation-actions">
                        <Link to={`/reservations/${r.id}/edit`} className="btn btn-teal">
                          Редагувати
                        </Link>
                        {isAdmin && (
                          <button
                            type="button"
                            className="btn btn-danger"
                            onClick={() => setToDelete(r)}
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
          ariaLabel="Навігація сторінками бронювань"
          onChange={setPageNumber}
        />
      </section>

      {toDelete && (
        <ConfirmDeleteModal
          title="Видалити це бронювання?"
          rows={[
            { label: "Гість", value: orNotSpecified(toDelete.guestFullName) },
            { label: "Номер", value: orDash(toDelete.roomNumber) },
            { label: "Заселення", value: formatDate(toDelete.checkIn) },
            { label: "Виселення", value: formatDate(toDelete.checkOut) },
          ]}
          warning="Після видалення бронювання його неможливо буде відновити."
          confirmLabel="Видалити бронювання"
          busy={deleting}
          onConfirm={confirmDelete}
          onClose={() => setToDelete(null)}
        />
      )}
    </div>
  );
}