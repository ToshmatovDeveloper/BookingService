namespace AuthService.Application.CustomException;

public class EmailIsAlreadyInUseException(string message) : Exception(message);