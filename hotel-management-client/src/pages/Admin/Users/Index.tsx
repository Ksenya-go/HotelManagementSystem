import { useEffect, useMemo, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { usersApi } from "@/api/usersApi";
import { useAuth } from "@/context/AuthContext";
import { getApiError } from "@/utils/apiError";
import { roleLabels, USER_ROLES, type UserRowDto } from "@/types/user";

interface Message {
  type: "success" | "danger";
  text: string;
}

function RoleSelect({
  onSave,
}: {
  onSave: (role: string) => Promise<void>;
}) {
  const [role, setRole] = useState<string>("Employee");
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await onSave(role);
    } finally {
      setSaving(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="d-flex align-items-center gap-2">
      <select
        value={role}
        onChange={(e) => setRole(e.target.value)}
        className="form-select form-select-sm"
      >
       {USER_ROLES.map((r: (typeof USER_ROLES)[number]) => (
          <option key={r} value={r}>
            {roleLabels[r]}
          </option>
        ))}
      </select>
      <button type="submit" className="btn btn-teal" disabled={saving}>
        Зберегти
      </button>
    </form>
  );
}

export default function AdminUsersIndex() {
  const { user: me } = useAuth();
  const location = useLocation();
  const flash = (location.state as { success?: string } | null)?.success;

  const [users, setUsers] = useState<UserRowDto[]>([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState<Message | null>(
    flash ? { type: "success", text: flash } : null
  );

  const load = async () => {
    try {
      setUsers(await usersApi.list());
    } catch {
      setMessage({ type: "danger", text: "Не вдалося завантажити користувачів." });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const totalUsers = users.length;
  const activeUsers = users.filter((u) => !u.isLocked).length;
  const blockedUsers = users.filter((u) => u.isLocked).length;
  const adminUsers = users.filter((u) => u.role === "Admin").length;

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return users;
    return users.filter((u) => `${u.fullName} ${u.email}`.toLowerCase().includes(q));
  }, [users, search]);

  const toggleLockout = async (target: UserRowDto) => {
    setMessage(null);
    if (target.id === me?.id) {
      setMessage({ type: "danger", text: "Не можна заблокувати власний обліковий запис." });
      return;
    }
    try {
      await usersApi.toggleLockout(target.id);
      await load();
    } catch (err) {
      setMessage({
        type: "danger",
        text: getApiError(err, "Не вдалося виконати операцію з користувачем."),
      });
    }
  };

  const changeRole = async (target: UserRowDto, role: string) => {
    setMessage(null);
    try {
      await usersApi.changeRole(target.id, role);
      await load();
    } catch (err) {
      setMessage({ type: "danger", text: getApiError(err, "Оберіть доступну роль.") });
    }
  };

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Користувачі</h1>
          <p className="page-subtitle">Керуйте працівниками готелю та їхніми дозволами.</p>
        </div>
      </div>

      {message && (
        <div className={`alert app-alert alert-${message.type}`}>{message.text}</div>
      )}

      <div className="stat-grid stat-grid-four">
        <div className="stat-card">
          <div className="stat-icon teal-icon">
            <i className="bi bi-people" />
          </div>
          <span className="stat-label">Людей у команді</span>
          <span className="stat-value">{totalUsers}</span>
          <span className="stat-muted">з доступом</span>
        </div>

        <div className="stat-card">
          <div className="stat-icon green-icon">
            <i className="bi bi-person-check" />
          </div>
          <span className="stat-label">Активний</span>
          <span className="stat-value">{activeUsers}</span>
          <span className="stat-trend positive">онлайн</span>
        </div>

        <div className="stat-card">
          <div className="stat-icon amber-icon">
            <i className="bi bi-person-x" />
          </div>
          <span className="stat-label">Заблокований</span>
          <span className="stat-value">{blockedUsers}</span>
          <span className="stat-muted">без доступу</span>
        </div>

        <div className="stat-card">
          <div className="stat-icon blue-icon">
            <i className="bi bi-shield-check" />
          </div>
          <span className="stat-label">Адміністратор</span>
          <span className="stat-value">{adminUsers}</span>
          <span className="stat-muted">повні права</span>
        </div>
      </div>

      <section className="content-card table-card">
        <div className="card-heading">
          <div>
            <h2>Працівники</h2>
          </div>

          <div className="table-tools d-flex align-items-center gap-2">
            <input
              className="form-control search-input"
              placeholder="Пошук користувачів..."
              aria-label="Пошук користувачів..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
            <Link to="/admin/users/new" className="btn btn-teal">
              <i className="bi bi-plus-lg" /> Додати працівника
            </Link>
          </div>
        </div>

        {loading ? (
          <p style={{ padding: "0 23px 22px" }}>Завантаження...</p>
        ) : (
          <div className="table-responsive">
            <table className="hotel-table">
              <thead className="text-center">
                <tr>
                  <th>Працівник</th>
                  <th>Електронна пошта</th>
                  <th>Роль</th>
                  <th>Статус</th>
                  <th>Дія</th>
                </tr>
              </thead>

              <tbody className="text-center">
                {filtered.map((u) => {
                  const initial = u.fullName.trim() ? u.fullName.trim()[0].toUpperCase() : "?";

                  return (
                    <tr key={u.id}>
                      <td>
                        <div className="person-cell">
                          <div
                            className={`avatar avatar-small ${
                              u.role === "Admin" ? "avatar-purple" : ""
                            }`}
                          >
                            {initial}
                          </div>
                          <strong>{u.fullName}</strong>
                        </div>
                      </td>

                      <td>{u.email}</td>

                      <td>
                        {u.role === "Admin" ? (
                          <span className="status-badge status-blue">Адміністратор</span>
                        ) : adminUsers === 0 ? (
                          <RoleSelect onSave={(role) => changeRole(u, role)} />
                        ) : (
                          <span className="status-badge status-blue">Працівник готелю</span>
                        )}
                      </td>

                      <td>
                        <span
                          className={`status-badge ${
                            u.isLocked ? "status-red" : "status-green"
                          }`}
                        >
                          <i
                            className={`bi ${
                              u.isLocked ? "bi-lock-fill" : "bi-check-circle-fill"
                            }`}
                          />
                          {u.isLocked ? "Заблокований" : "Активний"}
                        </span>
                      </td>

                      <td>
                        <div className="d-flex align-items-center justify-content-center gap-2">
                          <Link to={`/admin/users/${u.id}/edit`} className="btn btn-teal">
                            Редагувати
                          </Link>

                          <button
                            type="button"
                            className={`btn btn-danger ${
                              u.isLocked ? "btn-outline-success" : "btn-outline-danger"
                            }`}
                            onClick={() => toggleLockout(u)}
                          >
                            {u.isLocked ? "Розблокувати" : "Заблокувати"}
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>

            {users.length === 0 && (
              <div className="notice-card">
                <div className="notice-icon">
                  <i className="bi bi-people" />
                </div>
                <div>
                  <strong>Користувачів ще немає</strong>
                  <p>Додайте першого працівника, щоб надати йому доступ.</p>
                </div>
                <Link to="/admin/users/new" className="btn btn-teal">
                  <i className="bi bi-plus-lg" /> Додати працівника
                </Link>
              </div>
            )}
          </div>
        )}
      </section>
    </div>
  );
}