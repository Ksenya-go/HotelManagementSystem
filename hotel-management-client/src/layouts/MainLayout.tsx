import { useState } from "react";
import { NavLink, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";

export default function MainLayout() {
  const { user, logout, hasRole } = useAuth();
  const { pathname } = useLocation();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const isAdmin = hasRole("Admin");
  const roleName = isAdmin ? "Системний адміністратор" : "Працівник готелю";
  const avatarLetter = user?.fullName?.trim()?.[0]?.toUpperCase() ?? "К";

  // "/rooms" не має підсвічуватись на "/rooms/booking"
  const roomsListActive = !pathname.startsWith("/rooms/booking");

  return (
    <div className="app-body">
      <aside className={`app-sidebar ${sidebarOpen ? "sidebar-open" : ""}`} id="app-sidebar">
        <div className="brand-lockup">
          <div className="brand-mark" aria-hidden="true">Г</div>
          <div>
            <strong>
              Готель<span>Плюс</span>
            </strong>
            <small>Платформа управління</small>
          </div>
        </div>

        <div className="sidebar-section-label">УПРАВЛІННЯ</div>

        <nav
          className="sidebar-nav"
          aria-label="Основна навігація"
          onClick={() => setSidebarOpen(false)}
        >
          <NavLink to="/reservations" className="sidebar-link">
            Список бронювань
          </NavLink>

          <NavLink to="/rooms/booking" className="sidebar-link">
            Бронювання номерів
          </NavLink>

          <NavLink
            to="/rooms"
            className={({ isActive }) =>
              `sidebar-link ${isActive && roomsListActive ? "active" : ""}`
            }
          >
            Список номерів
          </NavLink>

          {hasRole("Employee", "Admin") && (
            <NavLink to="/reports" className="sidebar-link">
              Аналітика
            </NavLink>
          )}

          {isAdmin && (
            <>
              <div className="sidebar-section-label sidebar-section-spaced">
                Адміністрування
              </div>

              <NavLink to="/admin/users" className="sidebar-link">
                Користувачі
              </NavLink>

              <NavLink to="/admin/settings" className="sidebar-link">
                Налаштування
              </NavLink>
            </>
          )}
        </nav>
      </aside>

      <div className="app-main">
        <header className="app-topbar">
          <button
            className="mobile-menu-button"
            type="button"
            aria-label="Відкрити меню"
            onClick={() => setSidebarOpen((open) => !open)}
          >
            <i className="bi bi-list" />
          </button>

          <div className="topbar-actions">
            <button type="button" className="logout-button" onClick={logout}>
              <span>Вийти з системи</span>
            </button>

            <div className="user-menu">
              <div className="avatar">{avatarLetter}</div>
              <div className="user-copy">
                <strong>{user?.fullName}</strong>
                <small>{roleName}</small>
              </div>
            </div>
          </div>
        </header>

        <main className="app-content" role="main">
          <Outlet />
        </main>
      </div>
    </div>
  );
}