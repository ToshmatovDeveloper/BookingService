namespace AuthService.Application.CustomException;

public class FailedAddUserRoleException(string message) : Exception(message);