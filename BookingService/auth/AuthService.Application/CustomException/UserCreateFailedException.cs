namespace AuthService.Application.CustomException;

public class UserCreateFailedException(string message) : Exception(message);