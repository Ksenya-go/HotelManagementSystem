using HotelManagementSystem.Application.RoomTypes;
using HotelManagementSystem.Application.SystemSettings;
using HotelManagementSystem.Persistence.EfCore.Identity;
using HotelManagementSystem.Persistence.EfCore.SystemSettings;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using HotelManagementSystem.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Application.RoomTypes.Handlers;
using HotelManagementSystem.Application.Rooms.Handlers;
using HotelManagementSystem.Application.SystemSettings.Handlers;
using FluentValidation;
using HotelManagementSystem.Application.RoomTypes.Queries;
using HotelManagementSystem.Application.RoomTypes.Commands;
using HotelManagementSystem.Application.SystemSettings.Queries;
using HotelManagementSystem.Application.SystemSettings.Commands;
using FluentResults;
using HotelManagementSystem.Application.Guests.Queries;
using HotelManagementSystem.Application.Guests.Handlers;
using HotelManagementSystem.Application.Guests;
using HotelManagementSystem.Application.Reservations.Commands;
using HotelManagementSystem.Application.Reservations.Handlers;
using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Application.Rooms.Commands;
using HotelManagementSystem.Application.Reservations.Queries;
using HotelManagementSystem.Application.Reservations;
using HotelManagementSystem.Application.Common.Pagination;
using HotelManagementSystem.Application.Guests.Commands;
using HotelManagementSystem.Persistence.EfCore.Guests;
using HotelManagementSystem.Persistence.EfCore.Rooms;
using HotelManagementSystem.Persistence.EfCore.Reservations;
using HotelManagementSystem.Persistence.EfCore.Dashboard;
using HotelManagementSystem.Web.ViewModels.Rooms;
using HotelManagementSystem.Web.ViewModels.Admin;
using Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IRoomAvailabilityService, RoomAvailabilityService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IManagerReportingService, ManagerReportingService>();
builder.Services.AddScoped<RoomListItemViewModelFactory>();
builder.Services.AddScoped<SystemSettingItemViewModelFactory>();
builder.Services.AddScoped<ICommandHandler<CreateGuestCommand, Result<GuestDto>>, GuestCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateGuestCommand, Result<Unit>>, GuestCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CreateReservationCommand, Result<ReservationDto>>, ReservationCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateReservationCommand, Result<Unit>>, ReservationCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeReservationStatusCommand, Result<Unit>>, ReservationCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CancelReservationCommand, Result<Unit>>, ReservationCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteReservationCommand, Result<Unit>>, ReservationCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CheckInReservationCommand, Result<Unit>>, ReservationCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CheckOutReservationCommand, Result<Unit>>, ReservationCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetGuestsQuery, Result<IReadOnlyList<GuestDto>>>, GetGuestsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetRoomsQuery, Result<PagedResult<RoomDto>>>, GetRoomsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateRoomCommand, Result<IReadOnlyList<RoomDto>>>, RoomCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateRoomCommand, Result<Unit>>, RoomCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteRoomCommand, Result<Unit>>, RoomCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeRoomStatusCommand, Result<Unit>>, RoomCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetReservationsQuery, Result<PagedResult<ReservationDto>>>, GetReservationsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetRoomTypesQuery, Result<IReadOnlyList<RoomTypeDto>>>, RoomTypeQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateRoomTypeCommand, Result<RoomTypeDto>>, RoomTypeCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateRoomTypeCommand, Result<Unit>>, RoomTypeCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteRoomTypeCommand, Result<Unit>>, RoomTypeCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetSystemSettingsQuery, Result<IReadOnlyList<SystemSettingDto>>>, SystemSettingQueryHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateSystemSettingCommand, Result<Unit>>, SystemSettingCommandHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateGuestCommand.Validator>();

var app = builder.Build();

var supportedCultures = new[] { new CultureInfo("uk-UA") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("uk-UA"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders = new IRequestCultureProvider[]
    {
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();

if (app.Environment.IsDevelopment())
{

}

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    await IdentitySeed.SeedAsync(scope.ServiceProvider, app.Configuration);
    await DemoDataSeed.SeedAsync(scope.ServiceProvider, app.Configuration);
}
app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Type"] = "text/html; charset=utf-8";
    await next();
});


app.Run();

public partial class Program;

