using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Core.Repositories;
using Portal.Services.OauthService;
using Portal.Services.OauthService.Exceptions;
using Xunit;

namespace UnitTests.Services;

public class OauthServiceUnitTests
{
    private readonly IOauthService _oauthService;
    private readonly Mock<IUserRepository> _mockUserRepository = new();

    private const Role Permissions = Role.Administrator;
    
    public OauthServiceUnitTests()
    {
        _oauthService = new OauthService(_mockUserRepository.Object,
            NullLogger<OauthService>.Instance);
    }

    [Fact]
    public async Task RegistrationTest()
    {
        // Arrange
        // var login = "login1vc";
        var password = "password";
        var users = CreateMockUsers();

        var expectedUser = CreateUser(Guid.NewGuid());
        expectedUser.CreateHash(password);

        _mockUserRepository
            .Setup(s => s.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<Role>()))
            .ReturnsAsync((string l, Role p) => users.First(u => u.Email == l));

        _mockUserRepository.Setup(s => s.InsertUserAsync(It.IsAny<User>(), It.IsAny<Role>()))
                           .Callback((User u, Role p) => users.Add(u));

        // Act

        await _oauthService.Registrate(expectedUser, password);
        var actualUser = users.Last();

        // Assert
        Assert.Equal(expectedUser, actualUser);
    }

    [Fact]
    public async Task RegistrationEmptyTest()
    {
        // Arrange
        var login = "login";
        var password = "password";
        var users = new List<User>();

        var expectedUser = CreateUser(Guid.NewGuid());
        expectedUser.CreateHash(password);

        _mockUserRepository
            .Setup(s => s.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<Role>()))
            .ReturnsAsync((string l, Role p) => users.First(u => u.Email == l));

        _mockUserRepository.Setup(s => s.InsertUserAsync(It.IsAny<User>(), It.IsAny<Role>()))
                           .Callback((User u, Role p) => users.Add(u));

        // Act

        await _oauthService.Registrate(expectedUser, password);
        var actualUser = users.Last();

        // Assert
        Assert.Equal(expectedUser, actualUser);
    }

    [Fact]
    public async Task RegistrationExistsLoginTest()
    {
        // Arrange
        var login = "login";
        var password = "password";
        var users = CreateMockUsers();

        var user = users.First();
        user.CreateHash(password);

        _ = _mockUserRepository
            .Setup(s => s.GetUserByEmailAsync(login, It.IsAny<Role>()))
            .ReturnsAsync(users.First(e => e.Email == login));

        // Act

        async Task Action() => await _oauthService.Registrate(user, password);
        var actualUser = users.Last();

        // Assert
        await Assert.ThrowsAsync<UserLoginAlreadyExistsException>(Action);
    }

    [Fact]
    public async Task LogInIncorrectPasswordTest()
    {
        // Arrange
        var login = "ivanov@mail.ru";
        var password = "password123";
        var users = CreateMockUsers();

        var expectedUser = users[0];
        _mockUserRepository
            .Setup(s => s.GetUserByEmailAsync(login, It.IsAny<Role>()))
            .ReturnsAsync(users.First(e => e.Email == login));

        // Act
        Task<User> Action() => _oauthService.LogIn(login, password);

        // Assert
        await Assert.ThrowsAsync<IncorrectPasswordException>(Action);
    }

    [Fact]
    public async Task LogInNotFoundUserTest()
    {
        // Arrange
        var login = "login123";
        var password = "password";
        var users = CreateMockUsers();

        var expectedUser = users[0];
        _mockUserRepository
            .Setup(s => s.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<Role>()))
            .ReturnsAsync((string l, Role p) => users.First(u => u.Email == l));

        // Act
        Task<User> Action() => _oauthService.LogIn(login, password);

        // Assert
        await Assert.ThrowsAsync<UserLoginNotFoundException>(Action);
    }

    [Fact]
    public async Task LogInOkTest()
    {
        // Arrange
        var login = "login";
        var password = "password";
        var users = CreateMockUsers();

        var expectedUser = users.First();
        _mockUserRepository
            .Setup(s => s.GetUserByEmailAsync(login, It.IsAny<Role>()))
            .ReturnsAsync(users.Find(e => e.Email == login)!);

        // Act
        var actualUser = await _oauthService.LogIn(login, password);

        // Assert
        Assert.Equal(expectedUser, actualUser);
    }
    
    private User CreateUser(Guid userId)
    {
        return new User(userId, "Иванов", "Иван", "Иванович", new DateOnly(1998, 05, 17), Gender.Male, "ivanovvv@mail.ru", "+78888889");
    }
    
    private List<User> CreateMockUsers()
    {
        return new List<User>()
        {
            new User(Guid.NewGuid(), "Иванов", "Иван", "Иванович",  new DateOnly(2002, 03, 17), Gender.Male, "login", "+7899999999", "$2a$11$2RL0J3feilWw6859UcNwY.dighT4cPxG/she0Omtu36eVtamkV.8y"),
            new User(Guid.NewGuid(), "Иванов", "Иван", "Иванович",  new DateOnly(2002, 03, 17), Gender.Male, "ivanov@mail.ru", "+7899999999", "$2a$11$2RL0J3feilWw6859UcNwY.dighT4cPxG/she0Omtu36eVtamkV.8y"),
            new User(Guid.NewGuid(), "Петров", "Петр", "Петрович",  new DateOnly(2003, 05, 18), Gender.Male, "petrov@mail.ru", "+7899909999", "$2a$11$2RL0J3feilWw6859UcNwY.dighT4cPxG/she0Omtu36eVtamkV.8y"),
            new User(Guid.NewGuid(), "Cударь", "Елена", "Александровна",  new DateOnly(1999, 09, 18), Gender.Female, "sudar@mail.ru", "+781211111", "$2a$11$2RL0J3feilWw6859UcNwY.dighT4cPxG/she0Omtu36eVtamkV.8y")
        };
    }
}