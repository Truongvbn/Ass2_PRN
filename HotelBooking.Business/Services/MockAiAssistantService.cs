using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;

namespace HotelBooking.Business.Services;

/// <summary>
/// Mock AI assistant for demo. Swap with real LLM (OpenAI/Gemini) by replacing this implementation.
/// </summary>
public class MockAiAssistantService : IAiAssistantService
{
    private readonly IRoomService _roomService;

    public MockAiAssistantService(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task<ServiceResult<IReadOnlyList<RoomListDto>>> RecommendRoomsAsync(
        RoomPreferenceDto preferences, CancellationToken ct = default)
    {
        var result = await _roomService.SearchRoomsAsync(
            hotelId: null,
            roomTypeId: null,
            minPrice: null,
            maxPrice: preferences.MaxBudget,
            minOccupancy: preferences.GuestCount,
            checkIn: preferences.CheckIn,
            checkOut: preferences.CheckOut,
            ct);

        if (!result.IsSuccess)
            return ServiceResult<IReadOnlyList<RoomListDto>>.Failure(result.ErrorMessage!, result.ErrorCode);

        var rooms = result.Data!;

        // Simple ranking: prefer rooms with amenities matching preferences
        if (preferences.DesiredAmenities is { Count: > 0 })
        {
            var ranked = rooms.OrderByDescending(r =>
                preferences.DesiredAmenities.Count(a =>
                    r.Name.Contains(a, StringComparison.OrdinalIgnoreCase)))
                .Take(5)
                .ToList();
            return ServiceResult<IReadOnlyList<RoomListDto>>.Success(ranked);
        }

        return ServiceResult<IReadOnlyList<RoomListDto>>.Success(rooms.Take(5).ToList());
    }

    public Task<ServiceResult<string>> AnswerQuestionAsync(string question, int? roomId, CancellationToken ct = default)
    {
        // Mock responses
        var answer = question.ToLower() switch
        {
            var q when q.Contains("check-in") || q.Contains("checkin") =>
                "Check-in time is 2:00 PM and check-out time is 12:00 PM. Early check-in and late check-out are available upon request.",
            var q when q.Contains("parking") =>
                "We offer complimentary valet parking for all guests. Self-parking is also available for $15/day.",
            var q when q.Contains("breakfast") =>
                "Breakfast buffet is served daily from 6:30 AM to 10:30 AM in our main restaurant. Room service breakfast is also available.",
            var q when q.Contains("pool") || q.Contains("spa") =>
                "Our infinity pool is open from 7 AM to 10 PM. The spa offers treatments from 9 AM to 9 PM. Advance booking is recommended.",
            var q when q.Contains("wifi") || q.Contains("internet") =>
                "Complimentary high-speed WiFi is available throughout the hotel for all guests.",
            var q when q.Contains("cancel") =>
                "Free cancellation is available up to 24 hours before check-in. Cancellations within 24 hours may incur a one-night charge.",
            _ => "Thank you for your question! For detailed assistance, please create a support ticket or contact our front desk at +84 123 456 789."
        };

        return Task.FromResult(ServiceResult<string>.Success(answer));
    }
}
