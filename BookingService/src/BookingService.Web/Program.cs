using BookingService.Application.Hotel.Create;
using BookingService.Application.Validation;
using BookingService.Application.Validation.Hotel;
using BookingService.Infrastructure;
using BookingService.Web.Middlewares;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); 

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddValidatorsFromAssembly(
    typeof(CreateHotelRequestValidator).Assembly);

builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>(); 

builder.Services.AddProblemDetails();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateHotelRequest).Assembly);
    
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "BookingService"); });
}
app.UseExceptionHandler();
app.MapControllers();
app.Run();