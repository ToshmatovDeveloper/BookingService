namespace BookingService.Auth.Application.CustomExceptions;

public class UserCreateFailedException(string message) : Exception(message);