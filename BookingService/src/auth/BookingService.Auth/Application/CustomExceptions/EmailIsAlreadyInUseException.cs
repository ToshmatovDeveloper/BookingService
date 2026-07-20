namespace BookingService.Auth.Application.CustomExceptions;

public class EmailIsAlreadyInUseException(string message) : Exception(message);