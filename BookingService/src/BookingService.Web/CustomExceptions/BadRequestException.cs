namespace BookingService.Web.CustomExceptions;

public class BadRequestException(string message) : Exception(message);