namespace BookingService.Auth.Application.CustomExceptions;

public class BadRequestException(string? message) : Exception(message);