namespace AuthService.Application.CustomException;

public class ValidationErrors(string propertyName, string errorMessage)
{
    public string PropertyName { get; set; } = propertyName;
    public string ErrorMessage { get; set; } = errorMessage;
}