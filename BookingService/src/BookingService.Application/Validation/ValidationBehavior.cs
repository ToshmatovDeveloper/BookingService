using BookingService.Application.CustomExceptions;
using FluentValidation;
using MediatR;

namespace BookingService.Application.Validation;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .Select(f => new ValidationErrors(
                f.PropertyName,
                f.ErrorMessage))
            .ToList();

        if (errors.Count != 0)
            throw new BadRequestException(errors.ToString());

        return await next(cancellationToken);
    }
}