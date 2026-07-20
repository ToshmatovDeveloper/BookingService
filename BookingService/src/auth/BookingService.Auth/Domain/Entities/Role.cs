using Microsoft.AspNetCore.Identity;

namespace BookingService.Auth.Domain.Entities;

public class Role(string name) : IdentityRole<Guid>(name);