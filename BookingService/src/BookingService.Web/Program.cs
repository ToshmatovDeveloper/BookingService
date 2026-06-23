using BookingService.Application.Hotel.Create;
using BookingService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); 

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateHotelRequest).Assembly);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "BookingService"); });
}

app.MapControllers();
app.Run();