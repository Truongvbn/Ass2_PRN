using AutoMapper;
using FluentAssertions;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using Moq;

namespace HotelBooking.Tests;

public class BookingServiceScopingTests
{
    [Fact]
    public async Task GetBookingsByHotelAsync_UsesRepositoryAndMapsResult()
    {
        var bookingRepo = new Mock<IBookingRepository>();
        var roomRepo = new Mock<IRoomRepository>();
        var mapper = new Mock<IMapper>();
        var hub = new Mock<IBookingHubNotifier>();

        bookingRepo.Setup(r => r.GetByHotelAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>
            {
                new()
                {
                    Id = 1,
                    Room = new Room { Id = 10, HotelId = 7, Hotel = new Hotel { Id = 7, Name = "H7" }, RoomType = new RoomType { Name = "Standard" }, Name = "R" },
                    User = new ApplicationUser { FullName = "U" }
                }
            });

        mapper.Setup(m => m.Map<IReadOnlyList<BookingDto>>(It.IsAny<object>()))
            .Returns(new List<BookingDto> { new() { Id = 1, HotelId = 7, HotelName = "H7" } });

        var service = new BookingService(bookingRepo.Object, roomRepo.Object, mapper.Object, hub.Object);

        var result = await service.GetBookingsByHotelAsync(7);

        result.IsSuccess.Should().BeTrue();
        bookingRepo.Verify(r => r.GetByHotelAsync(7, It.IsAny<CancellationToken>()), Times.Once);
        result.Data!.Should().HaveCount(1);
        result.Data![0].HotelId.Should().Be(7);
    }
}

