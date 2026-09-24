import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";

const navItem = ({ isActive }: { isActive: boolean }) =>
  `nav-link ${isActive ? "active" : ""}`;

export default function MainLayout() {
  const { user, logout, hasRole } = useAuth();

  return (
    <div className="app-shell">
      <header className="app-header">
        <nav className="app-nav">
          <NavLink to="/" className={navItem}>Головна</NavLink>
          <NavLink to="/rooms" className={navItem}>Номери</NavLink>
          <NavLink to="/reservations" className={navItem}>Бронювання</NavLink>
          <NavLink to="/guests" className={navItem}>Гості</NavLink>
          {hasRole("Manager", "Admin") && (
            <NavLink to="/manager-reports" className={navItem}>Звіти</NavLink>
          )}
          {hasRole("Admin") && (
            <>
              <NavLink to="/admin/users" className={navItem}>Користувачі</NavLink>
              <NavLink to="/admin/room-types" className={navItem}>Типи номерів</NavLink>
              <NavLink to="/admin/settings" className={navItem}>Налаштування</NavLink>
            </>
          )}
        </nav>
        <div className="app-user">
          {user && (
            <>
              <span>{user.fullName} ({user.role})</span>
              <button onClick={logout}>Вийти</button>
            </>
          )}
        </div>
      </header>

      <main className="app-content">
        <Outlet />
      </main>

      <footer className="app-footer">
        &copy; {new Date().getFullYear()} Hotel Management System
      </footer>
    </div>
  );
}