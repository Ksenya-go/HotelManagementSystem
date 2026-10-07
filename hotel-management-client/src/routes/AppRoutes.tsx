import { Navigate, Route, Routes } from "react-router-dom";
import ProtectedRoute from "@/components/ProtectedRoute";
import MainLayout from "@/layouts/MainLayout";
import NotFound from "@/pages/NotFound";
import Login from "@/pages/Account/Login";
import RoomsIndex from "@/pages/Rooms/Index";
import RoomCreate from "@/pages/Rooms/Create";
import RoomEdit from "@/pages/Rooms/Edit";
import RoomBooking from "@/pages/Rooms/Booking";
import ReservationsIndex from "@/pages/Reservations/Index";
import ReservationCreate from "@/pages/Reservations/Create";
import ReservationEdit from "@/pages/Reservations/Edit";
import ReportsIndex from "@/pages/Reports/Index";
import AdminUsersIndex from "@/pages/Admin/Users/Index";
import AdminUserCreate from "@/pages/Admin/Users/Create";
import AdminUserEdit from "@/pages/Admin/Users/Edit";
import AdminSettingsIndex from "@/pages/Admin/Settings/Index";

const STAFF = ["Employee", "Admin"];

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />

      {/* Employee + Admin */}
      <Route element={<ProtectedRoute roles={STAFF} />}>
        <Route element={<MainLayout />}>
          <Route index element={<Navigate to="/reservations" replace />} />

          <Route path="reservations" element={<ReservationsIndex />} />
          <Route path="reservations/new" element={<ReservationCreate />} />
          <Route path="reservations/:id/edit" element={<ReservationEdit />} />

          <Route path="rooms" element={<RoomsIndex />} />
          <Route path="rooms/booking" element={<RoomBooking />} />
          <Route path="rooms/new" element={<RoomCreate />} />
          <Route path="rooms/:id/edit" element={<RoomEdit />} />

          <Route path="reports" element={<ReportsIndex />} />
        </Route>
      </Route>

      {/* Тільки Admin */}
      <Route element={<ProtectedRoute roles={["Admin"]} />}>
        <Route element={<MainLayout />}>
          <Route path="admin/users" element={<AdminUsersIndex />} />
          <Route path="admin/users/new" element={<AdminUserCreate />} />
          <Route path="admin/users/:id/edit" element={<AdminUserEdit />} />
          <Route path="admin/settings" element={<AdminSettingsIndex />} />
        </Route>
      </Route>

      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}