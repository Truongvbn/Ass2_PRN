using AutoMapper;
using FluentAssertions;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using Moq;
using HotelBooking.Business.Services.Interfaces;

namespace HotelBooking.Tests;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ITicketHubNotifier> _mockHub;
    private readonly TicketService _service;

    public TicketServiceTests()
    {
        _mockRepo = new Mock<ITicketRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockHub = new Mock<ITicketHubNotifier>();

        _service = new TicketService(_mockRepo.Object, _mockMapper.Object, _mockHub.Object);
    }

    [Fact]
    public async Task UpdateTicketStatusAsync_UnauthorizedUserToOpen_ReturnsForbidden()
    {
        // Arrange
        // Note: Customer cannot arbitrarily set their ticket to InProgress
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new SupportTicket { Id = 1, UserId = "customer-1", Status = TicketStatus.Open });

        // Act
        var result = await _service.UpdateTicketStatusAsync(1, "InProgress", "customer-1", isStaff: false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cannot transition from Open to InProgress");
    }

    [Fact]
    public async Task UpdateTicketStatusAsync_InvalidTransition_ReturnsError()
    {
        // Arrange
        // A Closed ticket cannot just be tossed directly to InProgress or Resolved
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new SupportTicket { Id = 1, Status = TicketStatus.Closed });

        // Act
        var result = await _service.UpdateTicketStatusAsync(1, "InProgress", "staff-1", isStaff: true);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cannot transition from Closed to InProgress");
    }

    [Fact]
    public async Task AssignTicketAsync_WhenTicketClosed_ReturnsError()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new SupportTicket { Id = 1, Status = TicketStatus.Closed });

        // Act
        var result = await _service.AssignTicketAsync(1, "staff-1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cannot assign a closed ticket");
    }
}
