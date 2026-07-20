namespace BookingService.Auth.Application.CustomExceptions;

public class FailedAddUserRoleException(string message) : Exception(message);