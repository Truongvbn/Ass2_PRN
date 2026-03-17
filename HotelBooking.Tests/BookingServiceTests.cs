using AutoMapper;
using FluentAssertions;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using Moq;
using HotelBooking.Business.Services.Interfaces;

namespace HotelBooking.Tests;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly Mock<IRoomRepository> _mockRoomRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBookingHubNotifier> _mockHub;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockHub = new Mock<IBookingHubNotifier>();

        _service = new BookingService(
            _mockBookingRepo.Object,
            _mockRoomRepo.Object,
            _mockMapper.Object,
            _mockHub.Object);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenRoomIsDeleted_ReturnsError()
    {
        // Arrange
        var dto = new CreateBookingDto { RoomId = 1, CheckIn = DateTime.UtcNow.Date.AddDays(1), CheckOut = DateTime.UtcNow.Date.AddDays(3), NumberOfGuests = 2 };
        
        _mockRoomRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Room { Id = 1, IsDeleted = true, IsAvailable = true });

        // Act
        var result = await _service.CreateBookingAsync(dto, "user-1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Room is not available");
    }

    [Fact]
    public async Task CreateBookingAsync_WhenDatesOverlap_ReturnsError()
    {
        // Arrange
        var dto = new CreateBookingDto { RoomId = 1, CheckIn = DateTime.UtcNow.Date.AddDays(1), CheckOut = DateTime.UtcNow.Date.AddDays(5), NumberOfGuests = 2 };
        
        _mockRoomRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Room { Id = 1, PricePerNight = 100, MaxOccupancy = 2, IsAvailable = true });
        
        // Mock overlapping booking exists
        _mockBookingRepo.Setup(r => r.HasOverlappingBookingAsync(1, dto.CheckIn, dto.CheckOut, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreateBookingAsync(dto, "user-1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Room is already booked for these dates");
    }

    [Fact]
    public async Task CreateBookingAsync_ValidData_CalculatesPriceCorrectlyAndReturnsSuccess()
    {
        // Arrange
        var dto = new CreateBookingDto { RoomId = 1, CheckIn = DateTime.UtcNow.Date, CheckOut = DateTime.UtcNow.Date.AddDays(4), NumberOfGuests = 2 }; // 4 nights
        
        _mockRoomRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Room { Id = 1, PricePerNight = 150, MaxOccupancy = 2, IsAvailable = true });
        _mockBookingRepo.Setup(r => r.HasOverlappingBookingAsync(1, dto.CheckIn, dto.CheckOut, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Capture saved booking
        Booking? savedBooking = null;
        _mockBookingRepo.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>((b, _) => savedBooking = b);

        // Act
        var result = await _service.CreateBookingAsync(dto, "user-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        savedBooking.Should().NotBeNull();
        savedBooking.TotalPrice.Should().Be(4 * 150); // 4 nights * $150
        savedBooking.Status.Should().Be(BookingStatus.AwaitingPayment); // no admin approval step
        savedBooking.PaymentDeadline.Should().NotBeNull(); // payment deadline set on creation
    }

    [Fact]
    public async Task CancelBookingAsync_WhenUserDiffers_ReturnsForbidden()
    {
        // Arrange
        _mockBookingRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Booking { Id = 1, UserId = "user-1", Status = BookingStatus.Pending });

        // Act
        var result = await _service.CancelBookingAsync(1, "different-user");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("You can only cancel your own bookings");
    }
}
