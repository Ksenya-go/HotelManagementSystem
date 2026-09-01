using FluentResults;
using Mediator;

namespace HotelManagementSystem.Application.Guests.Queries;

public sealed record GetGuestsQuery(string? Search = null) : IQuery<Result<IReadOnlyList<GuestDto>>>;

