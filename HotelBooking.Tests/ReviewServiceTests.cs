using AutoMapper;
using FluentAssertions;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using Moq;
using HotelBooking.Business.Services.Interfaces;

namespace HotelBooking.Tests;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _mockReviewRepo;
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly Mock<IReviewCommentRepository> _mockCommentRepo;
    private readonly Mock<IRoomRepository> _mockRoomRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IReviewHubNotifier> _mockHub;
    private readonly ReviewService _service;

    public ReviewServiceTests()
    {
        _mockReviewRepo = new Mock<IReviewRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockCommentRepo = new Mock<IReviewCommentRepository>();
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockHub = new Mock<IReviewHubNotifier>();

        _service = new ReviewService(
            _mockReviewRepo.Object,
            _mockCommentRepo.Object,
            _mockBookingRepo.Object,
            _mockMapper.Object,
            _mockHub.Object);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenUserHasNotStayed_ReturnsError()
    {
        // Arrange
        var dto = new CreateReviewDto { RoomId = 1, Rating = 5, Content = "This room was absolutely fantastic!" };
        _mockRoomRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Room { Id = 1 });
        
        // Mock user has zero completed bookings
        _mockBookingRepo.Setup(r => r.GetByUserAsync("user-1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _service.CreateReviewAsync(dto, "user-1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("You can only review rooms where you've completed a stay");
    }

    [Fact]
    public async Task CreateReviewAsync_WhenUserAlreadyReviewed_ReturnsError()
    {
        // Arrange
        var dto = new CreateReviewDto { RoomId = 1, Rating = 5, Content = "This room was absolutely fantastic!" };
        _mockRoomRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Room { Id = 1 });
        
        _mockBookingRepo.Setup(r => r.GetByUserAsync("user-1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<Booking> { new Booking { RoomId = 1, Status = BookingStatus.Completed } });
                        
        // Mock user already left a review
        _mockReviewRepo.Setup(r => r.HasUserReviewedRoomAsync("user-1", 1)).ReturnsAsync(true);

        // Act
        var result = await _service.CreateReviewAsync(dto, "user-1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("You have already reviewed this room");
    }

    [Fact]
    public async Task DeleteReviewAsync_ByNormalUserWhoDoesNotOwnIt_ReturnsError()
    {
        // Arrange
        _mockReviewRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Review { Id = 1, UserId = "owner-id" });

        // Act (Admin flag = false)
        var result = await _service.DeleteReviewAsync(1, "hacker-user", isAdmin: false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("You can only delete your own reviews");
    }

    [Fact]
    public async Task DeleteReviewAsync_ByAdmin_SoftDeletesCorrectly()
    {
        // Arrange
        var reviewToDel = new Review { Id = 1, UserId = "owner-id", IsDeleted = false };
        _mockReviewRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reviewToDel);
        _mockReviewRepo.Setup(r => r.UpdateAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);

        // Act (Admin flag = true)
        var result = await _service.DeleteReviewAsync(1, "admin-user", isAdmin: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        reviewToDel.IsDeleted.Should().BeTrue(); // Ensures it was soft-deleted, not completely wiped
    }
}
