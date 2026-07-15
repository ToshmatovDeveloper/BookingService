namespace AuthService.Application.CustomException;

public class UserNameIsAlreadyInUseException(string message) : Exception(message);