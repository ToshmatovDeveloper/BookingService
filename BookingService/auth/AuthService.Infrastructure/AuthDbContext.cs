using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) 
    : IdentityDbContext<Account, Role, Guid>(options)
{
    public DbSet<Account> Accounts { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.HasDefaultSchema("auth");
        builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}