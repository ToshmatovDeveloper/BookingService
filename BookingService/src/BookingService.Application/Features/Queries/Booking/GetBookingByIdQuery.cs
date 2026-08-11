using System.Text.Json.Serialization;
using BookingService.Application.CustomExceptions;
using BookingService.Application.Settings.Cache;
using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BookingService.Application.Features.Queries.Booking;

public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto>;

public class GetBookingByIdQueryHandler(
    IDistributedCache cache,
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

        string key = $"booking-{query.Id}";
        
        string? cachedBooking = await cache.GetStringAsync(key, cancellationToken);
        
        Domain.Entities.Booking? booking;
        
        if (string.IsNullOrEmpty(cachedBooking))
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
            
            await cache.SetStringAsync(
                key, 
                JsonConvert.SerializeObject(booking), 
                new DistributedCacheEntryOptions 
                    { AbsoluteExpirationRelativeToNow = options.CurrentValue.TimeToLive }, 
                cancellationToken);
            
            return new BookingDto(
                booking.HotelId,
                booking.RoomId,
                booking.StartDate,
                booking.EndDate);
        }
       
        logger.LogInformation($"Booking with id : {query.Id} found from cache.");
    
        booking = JsonConvert.DeserializeObject<Domain.Entities.Booking>(cachedBooking);
        
        return new BookingDto(
            booking!.HotelId,
            booking.RoomId,
            booking.StartDate,
            booking.EndDate);
    }

}