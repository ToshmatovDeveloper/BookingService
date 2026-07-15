namespace AuthService.Application.CustomException;

public class BadRequestException(string? message) : Exception(message);