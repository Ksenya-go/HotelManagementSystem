import { Routes, Route } from "react-router-dom";
import MainLayout from "@/layouts/MainLayout";
import ProtectedRoute from "@/components/ProtectedRoute";

import Login from "@/pages/Account/Login";
import Home from "@/pages/Home";

import RoomsIndex from "@/pages/Rooms/Index";
import RoomCreate from "@/pages/Rooms/Create";
import RoomEdit from "@/pages/Rooms/Edit";
import RoomBooking from "@/pages/Rooms/Booking";

import ReservationsIndex from "@/pages/Reservations/Index";
import ReservationCreate from "@/pages/Reservations/Create";
import ReservationEdit from "@/pages/Reservations/Edit";

import GuestsIndex from "@/pages/Guests/Index";

import AdminUsersIndex from "@/pages/Admin/Users/Index";
import AdminUserCreate from "@/pages/Admin/Users/Create";
import AdminUserEdit from "@/pages/Admin/Users/Edit";

import AdminRoomTypesIndex from "@/pages/Admin/RoomTypes/Index";
import AdminSettingsIndex from "@/pages/Admin/Settings/Index";

import ManagerReports from "@/pages/Manager/Reports";

import NotFound from "@/pages/NotFound";

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />

      <Route element={<MainLayout />}>
        <Route path="/" element={<Home />} />

        {/* Будь-який залогінений користувач */}
        <Route element={<ProtectedRoute />}>
          <Route path="/rooms" element={<RoomsIndex />} />
          <Route path="/rooms/:id/booking" element={<RoomBooking />} />

          <Route path="/reservations" element={<ReservationsIndex />} />
          <Route path="/reservations/new" element={<ReservationCreate />} />
          <Route path="/reservations/:id/edit" element={<ReservationEdit />} />

          <Route path="/guests" element={<GuestsIndex />} />
        </Route>

        {/* Manager + Admin */}
        <Route element={<ProtectedRoute roles={["Manager", "Admin"]} />}>
          <Route path="/rooms/new" element={<RoomCreate />} />
          <Route path="/rooms/:id/edit" element={<RoomEdit />} />
          <Route path="/manager-reports" element={<ManagerReports />} />
        </Route>

        {/* Тільки Admin */}
        <Route element={<ProtectedRoute roles={["Admin"]} />}>
          <Route path="/admin/users" element={<AdminUsersIndex />} />
          <Route path="/admin/users/new" element={<AdminUserCreate />} />
          <Route path="/admin/users/:id/edit" element={<AdminUserEdit />} />
          <Route path="/admin/room-types" element={<AdminRoomTypesIndex />} />
          <Route path="/admin/settings" element={<AdminSettingsIndex />} />
        </Route>

        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  );
}