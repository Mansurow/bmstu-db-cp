﻿using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Core.Repositories;
using Portal.Services.FeedbackService;
using Portal.Services.FeedbackService.Exceptions;
using Portal.Services.UserService.Exceptions;
using Portal.Services.ZoneService.Exceptions;

namespace UnitTests.Services;

public class FeedbackServiceUnitTests
{
    private readonly IFeedbackService _feedbackService;
    private readonly Mock<IFeedbackRepository> _mockFeedbackRepository = new();
    private readonly Mock<IZoneRepository> _mockZoneRepository = new();
    private readonly Mock<IUserRepository> _mockUserRepository = new();

    private const Role Permissions = Role.Administrator;
    
    public FeedbackServiceUnitTests()
    {
        _feedbackService = new FeedbackService(_mockFeedbackRepository.Object,
                                               _mockZoneRepository.Object,
                                               _mockUserRepository.Object,
                                               NullLogger<FeedbackService>.Instance);
    }

    [Fact]
    public async Task GetAllFeedbackOkTest()
    {
        // Arrange
        var zones = CreateMockZones();
        var users = CreateMockUsers();
        var feedbacks = CreateMockFeedback(zones, users);

        _mockFeedbackRepository.Setup(s => s.GetAllFeedbackAsync(It.IsAny<Role>()))
                               .ReturnsAsync(feedbacks);

        // Act
        var actualFeedbacks = await _feedbackService.GetAllFeedbackAsync(Permissions);
        
        // Assert
        Assert.Equal(feedbacks.Count, actualFeedbacks.Count);
        Assert.Equal(feedbacks, actualFeedbacks);
    }

    [Fact]
    public async Task GetAllFeedbackEmptyTest()
    {
        // Arrange
        var feedbacks = CreateEmptyMockFeedbacks();

        _mockFeedbackRepository.Setup(s => s.GetAllFeedbackAsync(It.IsAny<Role>()))
                               .ReturnsAsync(feedbacks);

        // Act

        var actualFeedbacks = await _feedbackService.GetAllFeedbackAsync(Permissions);

        // Assert
        Assert.Equal(feedbacks, actualFeedbacks);
    }

    [Fact]
    public async Task GetAllFeedbackByZoneOkTest()
    {
        // Arrange
        var zones = CreateMockZones();
        var users = CreateMockUsers();
        var feedbacks = CreateMockFeedback(zones, users);
        var zoneId = zones.First().Id;

        var exceptedFeedbacks = feedbacks.FindAll(e => e.ZoneId == zoneId);

        _mockZoneRepository.Setup(s => s.GetZoneByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                           .ReturnsAsync((Guid id, Role p) => zones.Find(e => e.Id == id)!);

        _mockFeedbackRepository.Setup(s => s.GetAllFeedbackByZoneAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                               .ReturnsAsync((Guid id, Role p) => feedbacks.FindAll(e => e.ZoneId == id));
        // Act
        var actualFeedbacks = await _feedbackService.GetAllFeedbackByZoneAsync(zoneId, Permissions);
        
        // Assert
        Assert.Equal(exceptedFeedbacks.Count, actualFeedbacks.Count);
        Assert.Equal(exceptedFeedbacks, actualFeedbacks);
    }

    [Fact]
    public async Task GetAllFeedbackByZoneEmptyTest()
    {
        // Arrange
        // var users = CreateMockUsers();
        var zones = CreateMockZones();
        var feedbacks = CreateEmptyMockFeedbacks();
        var zoneId = zones.First().Id;

        var expectedFeedbacks = feedbacks.Where(e => e.ZoneId == zoneId).ToList();

        _mockZoneRepository.Setup(s => s.GetZoneByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
            .ReturnsAsync((Guid id, Role p) => zones.Find(e => e.Id == id)!);

        _mockFeedbackRepository.Setup(s => s.GetAllFeedbackByZoneAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                               .ReturnsAsync((Guid id, Role p) => feedbacks.Where(e => e.ZoneId == id).ToList());
        // Act
        var actualFeedbacks = await _feedbackService.GetAllFeedbackByZoneAsync(zoneId, Permissions);

        // Assert
        Assert.Equal(expectedFeedbacks.Count, actualFeedbacks.Count);
        Assert.Equal(expectedFeedbacks, actualFeedbacks);
    }

    [Fact]
    public async Task GetAllFeedbackByZoneNotFoundTest()
    {
        // Arrange
        var zones = CreateMockZones();
        var zoneId = Guid.NewGuid();
    
        _mockZoneRepository.Setup(s => s.GetZoneByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                           .ReturnsAsync((Guid id, Role p) => zones.First(e => e.Id == id));
    
        // Act
        Task<List<Feedback>> Action() => _feedbackService.GetAllFeedbackByZoneAsync(zoneId, Permissions);

        // Assert
        await Assert.ThrowsAsync<ZoneNotFoundException>(Action);
    }
    
    [Fact]
    public async Task AddFeedbackOkTest()
    {
        // Arrange
        var users = CreateMockUsers();
        var zones = CreateMockZones();
        var feedbacks = CreateMockFeedback(zones, users);
    
        var zoneId = zones.Last().Id;
        var userId = users.First().Id;

        var feedbackId = Guid.NewGuid();
        var expectedFeedback = new Feedback(feedbackId, userId, zoneId, DateTime.UtcNow, 4.3, "Description1");
        
        _mockUserRepository.Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                           .ReturnsAsync(users.First());
    
        _mockZoneRepository.Setup(s => s.GetZoneByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                           .ReturnsAsync(zones.Last());
    
        _mockFeedbackRepository.Setup(s => s.InsertFeedbackAsync(It.IsAny<Feedback>(), It.IsAny<Role>()))
                               .Callback((Feedback f, Role p) => feedbacks.Add(f));
    
        // Act
        await _feedbackService.AddFeedbackAsync(expectedFeedback.ZoneId, expectedFeedback.UserId, 
            expectedFeedback.Mark, expectedFeedback.Message!, Permissions);
        var actualFeedback = feedbacks.Last();
    
        // Asser
        Assert.Equal(expectedFeedback.ZoneId, actualFeedback.ZoneId);
        Assert.Equal(expectedFeedback.UserId, actualFeedback.UserId);
        Assert.Equal(expectedFeedback.Mark, actualFeedback.Mark);
        Assert.Equal(expectedFeedback.Message, actualFeedback.Message);
    }
    
    [Fact]
    public async Task AddFeedbackEmptyTest()
    {
        // Arrange
        var users = CreateMockUsers();
        var zones = CreateMockZones();
        var feedbacks = CreateEmptyMockFeedbacks();
        
        var zoneId = zones.First().Id;
        var userId = users.First().Id;
        var feedbackId = Guid.NewGuid();
        
        var feedback = new Feedback(feedbackId, userId, zoneId, DateTime.UtcNow, 5, "Описание 1");
    
        _mockUserRepository.Setup(s => s.GetUserByIdAsync(userId, It.IsAny<Role>()))
                           .ReturnsAsync(users.First());
    
        _mockZoneRepository.Setup(s => s.GetZoneByIdAsync(zoneId, It.IsAny<Role>()))
                           .ReturnsAsync(zones.First());
    
        _mockFeedbackRepository.Setup(s => s.InsertFeedbackAsync(It.IsAny<Feedback>(), It.IsAny<Role>()))
                               .Callback((Feedback f, Role p) => feedbacks.Add(f));
    
        // Act
        await _feedbackService.AddFeedbackAsync(feedback.ZoneId, feedback.UserId, feedback.Mark, feedback.Message!, Permissions);
        var actualFeedback = feedbacks.Last();
    
        // Assert
        Assert.Equal(feedback.Mark, actualFeedback.Mark);
        Assert.Equal(feedback.Message, actualFeedback.Message);
        Assert.Equal(feedback.ZoneId, actualFeedback.ZoneId);
        Assert.Equal(feedback.UserId, actualFeedback.UserId);
    }
    
    [Fact]
    public async Task AddFeedbackNoExistUserTest()
    {
        // Arrange
        var users = CreateMockUsers();
        var zones = CreateMockZones();
        var feedbacks = CreateEmptyMockFeedbacks();
        
        var zoneId = zones.First().Id;
        var userId = Guid.NewGuid();
        var feedbackId = Guid.NewGuid();
        var expectedCount = feedbacks.Count;
        
        var feedback = new Feedback(feedbackId, userId, zoneId, DateTime.UtcNow, 5, "Описание 1");
    
        _mockUserRepository.Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
            .ReturnsAsync((Guid id, Role p) => users.First(u => u.Id == id));
        
        _mockZoneRepository.Setup(s => s.GetZoneByIdAsync(zoneId, It.IsAny<Role>()))
                           .ReturnsAsync(zones.First(e => e.Id == zoneId));
    
        _mockFeedbackRepository.Setup(s => s.InsertFeedbackAsync(It.IsAny<Feedback>(), It.IsAny<Role>()))
                               .Callback((Feedback f, Role p) => feedbacks.Add(f));
    
        // Act
        Task<Guid> Action() => _feedbackService.AddFeedbackAsync(feedback.ZoneId, Guid.NewGuid(), 
            feedback.Mark, feedback.Message!, Permissions);
        var actualCount = feedbacks.Count;
        
        // Assert
        Assert.Equal(expectedCount, actualCount);
        await Assert.ThrowsAsync<UserNotFoundException>(Action);
    }
    
    [Fact]
    public async Task AddFeedbackNotFoundRoomTest()
    {
        // Arrange
        var users = CreateMockUsers();
        var zones = CreateMockZones();
        var feedbacks = CreateEmptyMockFeedbacks();
        
        var zoneId = Guid.NewGuid();
        var userId = users.First().Id;
        var feedbackId = Guid.NewGuid();
        var expectedCount = feedbacks.Count;
    
        var feedback = new Feedback(feedbackId, userId, zoneId, DateTime.UtcNow, 5, "Описание 1");
    
        _mockZoneRepository.Setup(s => s.GetZoneByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                           .ReturnsAsync((Guid id, Role p) => zones.First(e => e.Id == id));
    
        // _mockFeedbackRepository.Setup(s => s.InsertFeedbackAsync(It.IsAny<Feedback>()))
        //                        .Callback((Feedback f) => feedbacks.Add(f));
    
        // Act
        Task<Guid> Action() => _feedbackService.AddFeedbackAsync(feedback.ZoneId, feedback.UserId, 
            feedback.Mark, feedback.Message!, Permissions);
        var actualCount = feedbacks.Count;
        
        // Assert
        Assert.Equal(expectedCount, actualCount);
        await Assert.ThrowsAsync<ZoneNotFoundException>(Action);
    }
    
    [Fact]
    public async Task UpdateFeedbackTest()
    {
        // Arrange
        var users = CreateMockUsers();
        var zones = CreateMockZones();
        var feedbacks = CreateMockFeedback(zones, users);
        
        var zoneId = feedbacks.First().ZoneId;
        var userId = feedbacks.First().UserId;
        var feedbackId = feedbacks.First().Id;

        var expectedDate = feedbacks.First().Date;
        var expectedFeedback = new Feedback(feedbackId, userId, zoneId, DateTime.UtcNow, 3, "Описание Новое");
        
        _mockFeedbackRepository.Setup(s => s.UpdateFeedbackAsync(It.IsAny<Feedback>(), It.IsAny<Role>()))
                           .Callback((Feedback f, Role p) =>
                           {
                               var feedback = feedbacks.Find(e => e.Id == f.Id)!;
                               feedback.Mark = f.Mark;
                               feedback.Message = f.Message;
                           });
    
        // Act
        await _feedbackService.UpdateFeedbackAsync(expectedFeedback, Permissions);
        var actualFeedback = feedbacks.Find(e => e.Id == feedbackId);
    
        // Assert
        Assert.Equal(expectedFeedback.Id, actualFeedback?.Id);
        Assert.Equal(expectedFeedback.Mark, actualFeedback?.Mark);
        Assert.Equal(expectedFeedback.Message, actualFeedback?.Message);
        Assert.Equal(expectedDate, actualFeedback?.Date);
        Assert.Equal(expectedFeedback.ZoneId, actualFeedback?.ZoneId);
        Assert.Equal(expectedFeedback.UserId, actualFeedback?.UserId);
    }
    
    [Fact]
    public async Task UpdateFeedbackEmptyTest()
    {
        // Arrange
        var users = CreateMockUsers();
        var zones = CreateMockZones();
        // var feedbacks = CreateEmptyMockFeedbacks();
        
        var zoneId = zones.First().Id;
        var userId = users.First().Id;
        var feedbackId = Guid.NewGuid();
    
        var feedback = new Feedback(feedbackId, userId, zoneId, DateTime.UtcNow, 3, "Описание Новое");
        
        _mockFeedbackRepository.Setup(s => s.UpdateFeedbackAsync(It.IsAny<Feedback>(), It.IsAny<Role>()))
            .ThrowsAsync(new InvalidOperationException());
        
        // Act
        async Task Action() => await _feedbackService.UpdateFeedbackAsync(feedback, Permissions);

        // Assert
        await Assert.ThrowsAsync<FeedbackNotFoundException>(Action);
    }
    
    [Fact]
    public async Task UpdateRoomRatingOkTest()
    {
        // Arrange
        var users = CreateMockUsers();
        var zones = CreateMockZones();

        var zoneId = zones.First().Id;
        var feedbacks = new List<Feedback>()
        {
            new Feedback(Guid.NewGuid(), users[2].Id, zoneId, new DateTime(2023, 03, 28), 4, "Описание1"),
            new Feedback(Guid.NewGuid(), users[0].Id, zoneId, new DateTime(2023, 02, 10), 3, "Описание2"),
            new Feedback(Guid.NewGuid(), users[1].Id, zoneId, new DateTime(2023, 02, 10), 3, "Описание3"),
            new Feedback(Guid.NewGuid(), users[1].Id, Guid.NewGuid(), new DateTime(2023, 02, 10), 0, "Описание3")
        };
        
        var exceptedRating = 3.3333333333333335;
    
        _mockZoneRepository.Setup(s => s.GetZoneByIdAsync(zoneId, It.IsAny<Role>()))
                               .ReturnsAsync(zones.Find(e => e.Id == zoneId)!);
    
        _mockFeedbackRepository.Setup(s => s.GetAllFeedbackByZoneAsync(zoneId, It.IsAny<Role>()))
                               .ReturnsAsync(feedbacks.FindAll(e => e.ZoneId == zoneId));
    
        _mockZoneRepository.Setup(s => s.UpdateZoneRatingAsync(It.IsAny<Guid>(), It.IsAny<double>(), Permissions))
                           .Callback((Guid id, double rating, Role p) =>
                           {
                               var zone = zones.Find(e => e.Id == id)!;
                               zone.Rating = rating;
                           });
    
        // Act
        await _feedbackService.UpdateZoneRatingAsync(zoneId, Permissions);
        var actualRating = zones.First().Rating;
    
        // Assert
        Assert.Equal(exceptedRating, actualRating);
    }
    
    [Fact]
    public async Task RemoveFeedbackOkTest()
    {
        // Arrange
        var users = CreateMockUsers();
        var zones = CreateMockZones();
        var feedbacks = CreateMockFeedback(zones, users);
        
        var feedbackId = feedbacks.Last().Id;
        var expectedCount = feedbacks.Count;
    
        _mockFeedbackRepository.Setup(s => s.GetFeedbackAsync(feedbackId, It.IsAny<Role>()))
                               .ReturnsAsync((Guid id, Role p) => feedbacks.First(f => f.Id == id));
    
        _mockFeedbackRepository.Setup(s => s.DeleteFeedbackAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                               .Callback((Guid id, Role p) => feedbacks.RemoveAll(e => e.Id == id));
    
        // Act
        await _feedbackService.RemoveFeedbackAsync(feedbackId, Permissions);
        var actualCount = feedbacks.Count;
    
        // Assert
        Assert.Equal(expectedCount - 1, actualCount);
    }
    
    [Fact]
    public async Task RemoveFeedbackEmptyTest()
    {
        // Arrange
        // var users = CreateMockUsers();
        // var zones = CreateMockZones();
        // var feedbacks = CreateMockFeedback(zones, users);
        
        var feedbackId = Guid.NewGuid();
        
        _mockFeedbackRepository.Setup(s => s.DeleteFeedbackAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
            .ThrowsAsync(new InvalidOperationException());
        
        // Act
        async Task Action() => await _feedbackService.RemoveFeedbackAsync(feedbackId, Permissions);

        // Assert
        await Assert.ThrowsAsync<FeedbackNotFoundException>(Action);
    }
    
    private List<Feedback> CreateMockFeedback(List<Zone> zones, List<User> users)
    {
        return new List<Feedback>()
        {
            new Feedback(Guid.NewGuid(), users[2].Id, zones[0].Id, new DateTime(2023, 03, 28), 4, "Описание1"),
            new Feedback(Guid.NewGuid(), users[0].Id, zones[1].Id, new DateTime(2023, 02, 10), 3, "Описание2"),
            new Feedback(Guid.NewGuid(), users[1].Id, zones[2].Id, new DateTime(2023, 02, 10), 3, "Описание3")
        };
    }

    private List<Feedback> CreateEmptyMockFeedbacks()
    {
        return new List<Feedback>();
    }

    private List<Zone> CreateMockZones()
    {
        return new List<Zone>
        {
            new Zone(Guid.NewGuid(), "Zone1", "address1", 10, 6, 4, new List<Inventory>(), new List<Package>()),
            new Zone(Guid.NewGuid(), "Zone2", "address2", 30, 6,  0.0, new List<Inventory>(), new List<Package>()),
            new Zone(Guid.NewGuid(), "Zone3", "address3", 25, 10,  0.0, new List<Inventory>(), new List<Package>()),
            new Zone(Guid.NewGuid(), "Zone3", "address3", 25, 10,  0.0, 
                CreateMockInventory(Guid.NewGuid()), new List<Package>())
        };
    }

    private List<User> CreateMockUsers()
    {
        return new List<User>()
        {
            new User(Guid.NewGuid(), "Иванов", "Иван", "Иванович",  new DateOnly(2002, 03, 17), Gender.Male, "ivanov@mail.ru", "+7899999999", "password12123434"),
            new User(Guid.NewGuid(), "Петров", "Петр", "Петрович",  new DateOnly(2003, 05, 18), Gender.Male, "petrov@mail.ru", "+7899909999", "password12122323"),
            new User(Guid.NewGuid(), "Cударь", "Елена", "Александровна",  new DateOnly(1999, 09, 18), Gender.Female, "sudar@mail.ru", "+781211111", "password12121212")
        };
    }
    
    private List<Inventory> CreateMockInventory(Guid zoneId)
    {
        return new List<Inventory>()
        {
            new Inventory(Guid.NewGuid(), zoneId, "Экран", "Просмотреть любимый фильм на экране диагональю 5 метров (можно принести свой фильм на флешке)", new DateOnly(2023, 03, 28)),
            new Inventory(Guid.NewGuid(), zoneId, "X-box/Playstation", "Поиграть на игровой приставке X-box/Playstation с сенсором движения kinect",  new DateOnly(2015, 10, 02)),
            new Inventory(Guid.NewGuid(), zoneId, "Just Dance", "Потанцевать с игрой Just Dance", new DateOnly(2002, 9, 10))
        };
    }
}
