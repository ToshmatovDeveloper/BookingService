using BookingService.Application.CustomExceptions;
using BookingService.Application.Settings.Cache;
using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingService.Application.Features.Queries.Booking;

public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto>;

public class GetBookingByIdQueryHandler(
    IMemoryCache cache,
    IOptionsMonitor<CacheSettings> options,
    ApplicationDbContext dbContext,
    ILogger<GetBookingByIdQueryHandler> logger) 
    : IRequestHandler<GetBookingByIdQuery, BookingDto>
{
    public async Task<BookingDto> Handle(
        GetBookingByIdQuery query,
        CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading booking by id : {query.Id}.");

        if (!cache.TryGetValue(query.Id, out Domain.Entities.Booking? booking))
        {
            booking = await dbContext.Bookings
                .Where(x => x.Id == query.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (booking == null)
            {
                logger.LogError($"Booking with id : {query.Id} not found.");
                throw new NotFoundException("Booking not found");
            }

            logger.LogInformation($"Booking with id : {query.Id} found from db.");

            cache.Set(
                query.Id,
                booking, options.CurrentValue.TimeToLive);
        }
        else
        {
            logger.LogInformation($"Booking with id : {query.Id} found from cache.");
        }
    
        return new BookingDto(
            booking!.HotelId,
            booking.RoomId,
            booking.StartDate,
            booking.EndDate);
    }

}