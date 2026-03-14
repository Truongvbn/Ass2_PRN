using AutoMapper;
using FluentAssertions;
using HotelBooking.Business.Services;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using Moq;

namespace HotelBooking.Tests;

public class RoomServiceScopingTests
{
    [Fact]
    public async Task SearchRoomsAsync_WhenHotelIdProvided_UsesSearchByHotelAsync()
    {
        var roomRepo = new Mock<IRoomRepository>();
        var roomTypeRepo = new Mock<IRoomTypeRepository>();
        var reviewRepo = new Mock<IReviewRepository>();
        var mapper = new Mock<IMapper>();

        roomRepo.Setup(r => r.SearchByHotelAsync(
                5, null, null, null, null, null, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Room>
            {
                new() { Id = 1, HotelId = 5, Hotel = new Hotel { Id = 5, Name = "H" }, RoomType = new RoomType { Name = "Standard" }, PricePerNight = 100, MaxOccupancy = 2, IsAvailable = true, ImageUrl = "x" }
            });

        reviewRepo.Setup(r => r.GetAverageRatingAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(4.5);

        var service = new RoomService(roomRepo.Object, roomTypeRepo.Object, reviewRepo.Object, mapper.Object);

        var result = await service.SearchRoomsAsync(5, null, null, null, null, null, null);

        result.IsSuccess.Should().BeTrue();
        roomRepo.Verify(r => r.SearchByHotelAsync(5, null, null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        roomRepo.Verify(r => r.SearchAsync(It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<int?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Never);
        result.Data!.Should().HaveCount(1);
        result.Data![0].HotelId.Should().Be(5);
        result.Data![0].HotelName.Should().Be("H");
    }

    [Fact]
    public async Task SearchRoomsAsync_WhenHotelIdNull_UsesSearchAsync()
    {
        var roomRepo = new Mock<IRoomRepository>();
        var roomTypeRepo = new Mock<IRoomTypeRepository>();
        var reviewRepo = new Mock<IReviewRepository>();
        var mapper = new Mock<IMapper>();

        roomRepo.Setup(r => r.SearchAsync(null, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Room>());

        var service = new RoomService(roomRepo.Object, roomTypeRepo.Object, reviewRepo.Object, mapper.Object);

        var result = await service.SearchRoomsAsync(null, null, null, null, null, null, null);

        result.IsSuccess.Should().BeTrue();
        roomRepo.Verify(r => r.SearchAsync(null, null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        roomRepo.Verify(r => r.SearchByHotelAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<int?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

