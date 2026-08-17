# HotelManagementSystem
A web-based hotel management system: room booking, guest records, room inventory and staff management.
This is a learning portfolio project focused on layered architecture, separation of concerns, clean business logic, and testability.

## Technologies

- ASP.NET Core MVC (.NET 10)
- Clean Architecture + DDD + CQRS
- PostgreSQL + Entity Framework Core
- ASP.NET Identity (Admin and Employee roles)
- Ukrainian localization via IStringLocalizer
- FluentValidation
- xUnit for testing
- Docker / Docker Compose

## Features

- Dashboard with operational statistics: room occupancy, current reservations, statuses
- Reservation management
- Room inventory management
- Filtering of rooms and reservations by relevant parameters
- Staff and access role management (for administrators)
- Hotel system settings
- Ukrainian-language interface

## Project structure

- HotelManagementSystem.Domain/ — domain entities, business rules
- HotelManagementSystem/ — CQRS commands and queries, DTOs, business logic (Application)
- HotelManagementSystem.Persistence.EfCore/ — data access, EF Core, migrations
- HotelManagementSystem.Web/ — MVC application, Razor views
- HotelManagementSystem.Web.Tests/ — tests

## How to run via Docker

1. Copy `.env.example` to `.env` and fill in your values: 
   Copy-Item .env.example .env
2. Run: 
   docker compose up
3. If this isn't your first run and you want to rebuild the image from scratch (e.g. after a dependency change):
   docker compose down -v
   docker compose build --no-cache
   docker compose up
4. The app will be available at http://localhost:8080.
On first startup, the hotelmanagementsystem.web container automatically applies EF Core migrations and seeds the database with test data (admin account, rooms, guests, reservations).

## Tests

dotnet test

## Environment variables

Configured via `.env` (see `.env.example`):
- DB_PASSWORD — PostgreSQL user password
- SEED_ADMIN_EMAIL — Email of the admin account created on first run
- SEED_ADMIN_PASSWORD — Password for the admin account
- SEED_ADMIN_FULLNAME — Full name of the admin account
- Seed__IncludeDemoData — true / false — whether to seed the database with demo data (rooms, guests, reservations)
