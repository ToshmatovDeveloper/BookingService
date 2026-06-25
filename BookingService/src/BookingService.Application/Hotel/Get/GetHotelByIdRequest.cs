using BookingService.Domain.DTOs;
using CSharpFunctionalExtensions;
using MediatR;

namespace BookingService.Application.Hotel.Get;

public record GetHotelByIdRequest(Guid Id) : IRequest<HotelDto>;