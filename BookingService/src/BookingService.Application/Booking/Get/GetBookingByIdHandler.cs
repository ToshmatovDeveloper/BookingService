using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using BookingService.Web.CustomExceptions;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Booking.Get;

public class GetBookingByIdHandler(
    IMemoryCache cache,
    ApplicationDbContext dbContext,
    ILogger<GetBookingByIdHandler> logger) 
    : IRequestHandler<GetBookingByIdRequest, BookingDto>
{
    public async Task<BookingDto> Handle(
        GetBookingByIdRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading booking by id : {request.Id}.");

        if (!cache.TryGetValue(request.Id, out Domain.Entities.Booking? booking))
        {
            booking = await dbContext.Bookings
                .Where(x => x.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (booking == null)
            {
                logger.LogError($"Booking with id : {request.Id} not found.");
                throw new NotFoundException("Booking not found");
            }

            logger.LogInformation($"Booking with id : {request.Id} found from db.");

            cache.Set(
                request.Id, 
                booking, 
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
        }
        else
        {
            logger.LogInformation($"Booking with id : {request.Id} found from cache.");
        }
    
        return new BookingDto(
            booking.HotelId,
            booking.RoomId,
            booking.StartDate,
            booking.EndDate);
    }

}