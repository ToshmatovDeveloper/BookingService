using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Booking.Get;

public class GetBookingByIdHandler(
    ApplicationDbContext dbContext,
    ILogger<GetBookingByIdHandler> logger) 
    : IRequestHandler<GetBookingByIdRequest, Result<BookingDto, string>>
{
    public async Task<Result<BookingDto, string>> Handle(
        GetBookingByIdRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading booking by id : {request.Id}");

        var result = await dbContext.Bookings
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            logger.LogError($"Booking with id : {request.Id} not found");

            return "Booking not found";
        }
        
        logger.LogInformation($"Booking with id : {request.Id} found");
        
        return new BookingDto(
            result.HotelId,
            result.RoomId,
            result.StartDate,
            result.EndDate);
    }
}