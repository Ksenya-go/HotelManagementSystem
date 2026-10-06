import { useEffect, useState } from "react";
import { reportsApi } from "@/api/reportsApi";
import type { ManagerReportDto, ManagerReportQuery } from "@/types/report";
import { addDays, firstOfMonthInput, todayInput } from "@/utils/date";

const percent = (part: number, total: number) =>
  total === 0 ? 0 : Math.round((part / total) * 1000) / 10;

interface DonutSegment {
  value: number; 
  className: string;
}

function Donut({
  segments,
  label,
  ariaLabel,
}: {
  segments: DonutSegment[];
  label: string | number;
  ariaLabel: string;
}) {
  let offset = 0;
  return (
    <div className="donut-chart report-donut" aria-label={ariaLabel}>
      <svg viewBox="0 0 42 42" role="img">
        <circle className="donut-track" cx="21" cy="21" r="15.915" />
        {segments.map((s, i) => {
          const circle = (
            <circle
              key={i}
              className={`donut-segment ${s.className}`}
              cx="21"
              cy="21"
              r="15.915"
              pathLength={100}
              strokeDasharray={`${s.value} 100`}
              strokeDashoffset={-offset}
            />
          );
          offset += s.value;
          return circle;
        })}
      </svg>
      <strong>{label}</strong>
    </div>
  );
}

export default function ReportsIndex() {
  const [from, setFrom] = useState(firstOfMonthInput());
  const [to, setTo] = useState(addDays(todayInput(), 1));
  const [report, setReport] = useState<ManagerReportDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = async (query: ManagerReportQuery) => {
    setLoading(true);
    setError(null);
    try {
      setReport(await reportsApi.get(query));
    } catch {
      setError("Не вдалося сформувати звіт.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load({ from, to });
  
  }, []);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    const fixedTo = to <= from ? addDays(from, 1) : to;
    setTo(fixedTo);
    load({ from, to: fixedTo });
  };

  const freeRoomDays = report
    ? Math.max(0, report.availableRoomDays - report.occupiedRoomDays)
    : 0;
  const activePercent = report ? percent(report.activeReservations, report.reservations) : 0;
  const completedPercent = report ? percent(report.completedReservations, report.reservations) : 0;
  const cancelledPercent = report ? percent(report.cancelledReservations, report.reservations) : 0;

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Аналітика готелю</h1>
          <p className="page-subtitle">
            Аналізуйте бронювання та заповненість за вибраний період.
          </p>
        </div>
      </div>

      <section className="content-card report-filter-card">
        <form className="report-filter-form" onSubmit={handleSubmit}>
          <div className="report-date-field">
            <label htmlFor="from" className="form-label">Від</label>
            <input
              id="from"
              type="date"
              className="form-control"
              value={from}
              onChange={(e) => setFrom(e.target.value)}
              required
            />
          </div>
          <div className="report-date-field">
            <label htmlFor="to" className="form-label">До</label>
            <input
              id="to"
              type="date"
              className="form-control"
              value={to}
              onChange={(e) => setTo(e.target.value)}
              required
            />
          </div>
          <button type="submit" className="btn btn-teal report-generate-btn" disabled={loading}>
            Сформувати <span>→</span>
          </button>
        </form>
      </section>

      {error && <div className="alert alert-danger">{error}</div>}
      {loading && !report && <p>Завантаження...</p>}

      {report && (
        <section className="report-dashboard">
          <div className="report-heading">
            <div>
              <span className="report-section-label">ОПЕРАЦІЙНИЙ ЗВІТ</span>
            </div>
            <div className="report-period-value">
              {report.from} — {report.to}
            </div>
          </div>

          <div className="report-analytics-grid">
          
            <article className="report-analytics-card">
              <div className="report-card-header">
                <div>
                  <span className="report-section-label">Номери готелю</span>
                  <h3>Завантаженість номерів</h3>
                </div>
              </div>
              <div className="report-chart-content">
                <Donut
                  segments={[{ value: report.occupancyRate, className: "donut-teal" }]}
                  label={`${report.occupancyRate}%`}
                  ariaLabel={`Завантаженість номерів: ${report.occupancyRate}%`}
                />
                <div className="report-chart-info">
                  <span className="report-stat-number">{report.occupiedRoomDays}</span>
                  <span className="report-stat-text">Зайнято номеро-днів</span>
                  <span className="report-stat-muted">
                    Вільний потенціал {freeRoomDays} номеро-днів
                  </span>
                </div>
              </div>
            </article>

            <article className="report-analytics-card">
              <div className="report-card-header">
                <div>
                  <span className="report-section-label">Список бронювань</span>
                  <h3>Структура статусів</h3>
                </div>
              </div>
              <div className="report-chart-content">
                <Donut
                  segments={[
                    { value: activePercent, className: "donut-teal" },
                    { value: completedPercent, className: "donut-blue" },
                    { value: cancelledPercent, className: "donut-red" },
                  ]}
                  label={report.reservations}
                  ariaLabel="Структура бронювань"
                />
                <div className="report-chart-legend">
                  <div className="report-legend-item">
                    <span>
                      <i className="legend-dot legend-teal" /> Активний
                    </span>
                    <strong>{report.activeReservations}</strong>
                  </div>
                  <div className="report-legend-item">
                    <span>
                      <i className="legend-dot legend-blue" /> Завершені
                    </span>
                    <strong>{report.completedReservations}</strong>
                  </div>
                  <div className="report-legend-item">
                    <span>
                      <i className="legend-dot legend-red" /> Скасовано
                    </span>
                    <strong>{report.cancelledReservations}</strong>
                  </div>
                </div>
              </div>
            </article>

          
            <article className="report-analytics-card">
              <div className="report-card-header">
                <div>
                  <span className="report-section-label">РИЗИК</span>
                  <h3>Скасування бронювань</h3>
                </div>
              </div>
              <div className="report-chart-content">
                <Donut
                  segments={[{ value: report.cancellationRate, className: "donut-red" }]}
                  label={`${report.cancellationRate}%`}
                  ariaLabel={`Скасування: ${report.cancellationRate}%`}
                />
                <div className="report-chart-info">
                  <span className="report-stat-number">{report.cancelledReservations}</span>
                  <span className="report-stat-text">скасованих бронювань</span>
                  <span className="report-stat-muted">
                    із {report.reservations} загальних бронювань
                  </span>
                </div>
              </div>
            </article>

          
            <article className="report-analytics-card">
              <div className="report-card-header">
                <div>
                  <span className="report-section-label">ГОСТІ ТА ПОПИТ</span>
                  <h3>Поведінка гостей</h3>
                </div>
              </div>
              <div className="guest-stat-content">
                <div className="guest-stat-main">
                  <span className="guest-stat-number">{report.totalGuests}</span>
                  <span className="guest-stat-label">гостей за період</span>
                </div>
                <div className="guest-stat-divider" />
                <div className="guest-stat-meta">
                  <span>Середнє проживання</span>
                  <strong>{report.averageStayDays} дн.</strong>
                </div>
                <div className="guest-stat-meta">
                  <span>Вільний потенціал</span>
                  <strong>{freeRoomDays} номеро-днів</strong>
                </div>
              </div>
            </article>
          </div>
        </section>
      )}
    </div>
  );
}