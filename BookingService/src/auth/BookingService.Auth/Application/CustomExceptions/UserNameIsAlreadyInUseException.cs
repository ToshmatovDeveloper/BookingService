namespace BookingService.Auth.Application.CustomExceptions;

public class UserNameIsAlreadyInUseException(string message) : Exception(message);