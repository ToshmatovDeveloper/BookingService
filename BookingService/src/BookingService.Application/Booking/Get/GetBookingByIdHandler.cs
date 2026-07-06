using BookingService.Application.CustomExceptions;
using BookingService.Application.Settings.Cache;
using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingService.Application.Booking.Get;

public record GetBookingByIdRequest(Guid Id) : IRequest<BookingDto>;

public class GetBookingByIdHandler(
    IMemoryCache cache,
    IOptionsMonitor<CacheSettings> options,
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
                booking, options.CurrentValue.TimeToLive);
        }
        else
        {
            logger.LogInformation($"Booking with id : {request.Id} found from cache.");
        }
    
        return new BookingDto(
            booking!.HotelId,
            booking.RoomId,
            booking.StartDate,
            booking.EndDate);
    }

}