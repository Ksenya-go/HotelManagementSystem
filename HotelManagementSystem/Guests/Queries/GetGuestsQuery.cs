using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.Guests.Queries;

public sealed record GetGuestsQuery(string? Search = null) : IQuery<Result<IReadOnlyList<GuestDto>>>;

