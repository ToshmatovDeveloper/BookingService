using BookingService.Auth.Application.CustomExceptions;
using BookingService.Auth.Application.Features;
using BookingService.Auth.Application.Features.Tokens;
using BookingService.Auth.Application.Settings;
using BookingService.Auth.Domain.Entities;
using BookingService.Auth.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BookingService.IntegrationTests.authService;

public class AuthTests(
    AuthServiceTestWebFactory factory)
    : IClassFixture<AuthServiceTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase = factory.ResetDatabaseAsync;
    private IServiceProvider Services { get; set; } = factory.Services;

    [Fact]
    public async Task Register_with_valid_data_should_succeed_and_return_tokens()
    {
        // Arrange
        await using var scope = Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var command = new UserRegisterCommand(
            UserName: "testuser",
            Email: "testuser@mail.com",
            Password: "A!a12345678"
        );

        // Act
        var response = await mediator.Send(command, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.accessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.refreshToken));
        Assert.Equal("Welcome to Booking Service", response.message);
    }
    
    [Fact]
    public async Task Register_with_existing_username_should_throw_exception()
    {
        // Arrange
        await using var scope = Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();
        var cancellationToken = CancellationToken.None;

        var existingUserName = "existinguser";
        var existingEmail = "existing@mail.com";
        var password = "A!a12345678";

        var existingUser = new Account(existingEmail, existingUserName);
        var createResult = await userManager.CreateAsync(existingUser, password);
        Assert.True(createResult.Succeeded);

        var command = new UserRegisterCommand(
            UserName: existingUserName,
            Email: "newemail@mail.com",
            Password: password
        );

        // Act
        var exception = await Assert.ThrowsAsync<UserNameIsAlreadyInUseException>(async () =>
        {
            await mediator.Send(command, cancellationToken);
        });

        //Assert
        Assert.Equal("Username is already in use.", exception.Message);
    }
    
    
    [Fact]
    public async Task Login_with_valid_data_should_succeed()
    {
        // Arrange
        await using var scope = Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();
        var cancellationToken = CancellationToken.None;
        
        var email = "test@mail.com";
        var userName = "test";
        var password = "A!a12345678";

        var user = new Account(email, userName);
        
        var createResult = await userManager.CreateAsync(user, password);
        Assert.True(createResult.Succeeded, string.Join(", ", createResult.Errors.Select(e => e.Description)));
        
        var loginCommand = new UserLoginCommand(email, password);
        
        // Act
        var getResult = await mediator.Send(loginCommand, cancellationToken);
        
        // Assert
        Assert.NotNull(getResult);
        Assert.Equal(email, getResult.Email);
    }
    
    [Fact]
    public async Task Login_with_invalid_password_should_throw_unauthorized_exception()
    {
        // Arrange
        await using var scope = Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();
        var cancellationToken = CancellationToken.None;
        
        var email = "test@mail.com";
        var userName = "test";
        var correctPassword = "A!a12345678";
        var wrongPassword = "WrongPassword123!";

        var user = new Account(email, userName);
        var createResult = await userManager.CreateAsync(user, correctPassword);
        Assert.True(createResult.Succeeded, string.Join(", ", createResult.Errors.Select(e => e.Description)));
        
        var loginCommand = new UserLoginCommand(email, wrongPassword);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(async () =>
        {
            await mediator.Send(loginCommand, cancellationToken);
        });
        
        Assert.Equal("Invalid login or password.", exception.Message);
    }

    [Fact]
    public async Task Login_with_non_existent_email_should_throw_unauthorized_exception()
    {
        // Arrange
        await using var scope = Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;
        
        var loginCommand = new UserLoginCommand("nonexistent@mail.com", "A!a12345678");
        
        // Act 
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(async () =>
        {
            await mediator.Send(loginCommand, cancellationToken);
        });
        
        //Assert
        Assert.Equal("Invalid login or password.", exception.Message);
    }
    
    [Fact]
    public async Task RefreshToken_with_valid_token_should_succeed_and_rotate_tokens()
    {
        // Arrange
        await using var scope = Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();
        var tokenProvider = scope.ServiceProvider.GetRequiredService<TokenProvider>();
        var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<JwtSettings>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var cancellationToken = CancellationToken.None;

        var email = "refresh_test@mail.com";
        var password = "A!a12345678";

        var user = new Account(email, "refreshtest");
        var createResult = await userManager.CreateAsync(user, password);
        Assert.True(createResult.Succeeded);

        var originalRefreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            AccountId = user.Id,
            Token = tokenProvider.GenerateRefreshToken(),
            ExpiresOnUtc = DateTime.UtcNow.AddDays(jwtOptions.CurrentValue.RefreshTokenExpirationInDays)
        };

        await dbContext.RefreshTokens.AddAsync(originalRefreshToken, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var command = new RefreshTokenCommand(originalRefreshToken.Token);

        // Act
        var response = await mediator.Send(command, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.NotEqual(originalRefreshToken.Token, response.RefreshToken);

        var oldTokenInDb = await dbContext.RefreshTokens.FindAsync([originalRefreshToken.Id], cancellationToken);
        Assert.Null(oldTokenInDb);

        var newTokenInDb = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == response.RefreshToken, cancellationToken);
        Assert.NotNull(newTokenInDb);
        Assert.Equal(user.Id, newTokenInDb.AccountId);
    }

    [Fact]
    public async Task RefreshToken_with_invalid_token_should_throw_unauthorized_exception()
    {
        // Arrange
        await using var scope = Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var command = new RefreshTokenCommand("non-existent-or-fake-refresh-token");

        // Act 
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(async () =>
        {
            await mediator.Send(command, cancellationToken);
        });

        //Assert
        Assert.Equal("Invalid or expired refresh token.", exception.Message);
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }
}