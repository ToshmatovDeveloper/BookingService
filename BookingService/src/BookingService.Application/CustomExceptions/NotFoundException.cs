namespace BookingService.Web.CustomExceptions;

public class NotFoundException(string message) : Exception(message);