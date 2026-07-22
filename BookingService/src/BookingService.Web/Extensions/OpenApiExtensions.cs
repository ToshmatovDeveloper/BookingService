using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace BookingService.Web.Extensions;

public static class OpenApiExtensions
{
    public static void AddCustomOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                if (document == null) return Task.CompletedTask;

                var scheme = (IOpenApiSecurityScheme)new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme, 
                    BearerFormat = "JWT",
                    Description = "Введите ваш JWT access token в поле ниже"
                };

                document.Components ??= new OpenApiComponents();
    
                if (document.Components.SecuritySchemes == null)
                {
                    document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();
                }

                document.Components.SecuritySchemes["Bearer"] = scheme;

                return Task.CompletedTask;
            });


            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                if (operation == null || context?.Document == null) return Task.CompletedTask;

                var schemeRef = new OpenApiSecuritySchemeReference("Bearer", context.Document);
                var requirement = new OpenApiSecurityRequirement
                {
                    [schemeRef] = new List<string>()
                };

                operation.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Security.Add(requirement);

                return Task.CompletedTask;
            });
        });
    }
}
