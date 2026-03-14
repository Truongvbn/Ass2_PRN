using AutoMapper;
using FluentAssertions;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace HotelBooking.Tests;

public class HotelServiceTests
{
    private readonly Mock<IHotelRepository> _hotelRepo = new();
    private readonly Mock<IHotelStaffRepository> _hotelStaffRepo = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly Mock<IMapper> _mapper = new();

    private readonly HotelService _service;

    public HotelServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _service = new HotelService(
            _hotelRepo.Object,
            _hotelStaffRepo.Object,
            _userManager.Object,
            _mapper.Object);
    }

    [Fact]
    public async Task CreateHotelAsync_WhenNameMissing_ReturnsValidationError()
    {
        var result = await _service.CreateHotelAsync(new CreateHotelDto { Name = "  " });
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION");
    }

    [Fact]
    public async Task CreateHotelAsync_Valid_AddsHotelAndReturnsDto()
    {
        // Arrange
        var dto = new CreateHotelDto { Name = "H1", City = "C" };
        _mapper.Setup(m => m.Map<Hotel>(It.IsAny<CreateHotelDto>()))
            .Returns(new Hotel { Name = "H1" });

        Hotel? created = null;
        _hotelRepo.Setup(r => r.AddAsync(It.IsAny<Hotel>(), It.IsAny<CancellationToken>()))
            .Callback<Hotel, CancellationToken>((h, _) => { h.Id = 10; created = h; })
            .ReturnsAsync((Hotel h, CancellationToken _) => h);

        _hotelRepo.Setup(r => r.GetWithRoomsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Hotel { Id = 10, Name = "H1" });

        _mapper.Setup(m => m.Map<HotelDto>(It.IsAny<Hotel>()))
            .Returns(new HotelDto { Id = 10, Name = "H1" });

        // Act
        var result = await _service.CreateHotelAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        created.Should().NotBeNull();
        created!.IsActive.Should().BeTrue();
        result.Data!.Id.Should().Be(10);
        result.Data!.Name.Should().Be("H1");
    }

    [Fact]
    public async Task AssignStaffAsync_WhenAlreadyAssigned_ReturnsDuplicate()
    {
        _hotelRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Hotel { Id = 1, Name = "H" });
        _userManager.Setup(um => um.FindByIdAsync("u1"))
            .ReturnsAsync(new ApplicationUser { Id = "u1", FullName = "Staff" });
        _hotelStaffRepo.Setup(r => r.GetAssignmentAsync(1, "u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HotelStaff { HotelId = 1, UserId = "u1", Role = HotelStaffRole.Receptionist });

        var result = await _service.AssignStaffAsync(new AssignStaffDto { HotelId = 1, UserId = "u1", Role = "Manager" });

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("DUPLICATE");
    }

    [Fact]
    public async Task AssignStaffAsync_WhenRoleInvalid_DefaultsToReceptionist()
    {
        _hotelRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Hotel { Id = 1, Name = "H" });
        _userManager.Setup(um => um.FindByIdAsync("u1"))
            .ReturnsAsync(new ApplicationUser { Id = "u1", FullName = "Staff" });
        _hotelStaffRepo.Setup(r => r.GetAssignmentAsync(1, "u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((HotelStaff?)null);

        HotelStaff? saved = null;
        _hotelStaffRepo.Setup(r => r.AddAsync(It.IsAny<HotelStaff>(), It.IsAny<CancellationToken>()))
            .Callback<HotelStaff, CancellationToken>((hs, _) => saved = hs)
            .ReturnsAsync((HotelStaff hs, CancellationToken _) => hs);

        var result = await _service.AssignStaffAsync(new AssignStaffDto { HotelId = 1, UserId = "u1", Role = "NotARealRole" });

        result.IsSuccess.Should().BeTrue();
        saved.Should().NotBeNull();
        saved!.Role.Should().Be(HotelStaffRole.Receptionist);
    }
}

