namespace AuthService.Application.CustomException;

public class UnauthorizedException(string message) : Exception(message);
