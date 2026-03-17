using System.Text;
using System.Text.Json;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HotelBooking.Business.Services;

public class GeminiAiAssistantService : IAiAssistantService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IRoomService _roomService;

    public GeminiAiAssistantService(HttpClient httpClient, IConfiguration configuration, IRoomService roomService)
    {
        _httpClient = httpClient;
        _apiKey = configuration["AiAssistant:GeminiApiKey"] ?? "";
        _roomService = roomService;
    }

    public async Task<ServiceResult<IReadOnlyList<RoomListDto>>> RecommendRoomsAsync(RoomPreferenceDto preferences, CancellationToken ct = default)
    {
        // For recommendations, we still use the mock logic or basic search for now, 
        // as structured LLM output for room lists is more complex to implement without a library.
        var result = await _roomService.SearchRoomsAsync(null, null, null, preferences.MaxBudget, preferences.GuestCount, preferences.CheckIn, preferences.CheckOut, ct);
        if (!result.IsSuccess) return result;

        var rooms = result.Data!;
        return ServiceResult<IReadOnlyList<RoomListDto>>.Success(rooms.Take(5).ToList());
    }

    public async Task<ServiceResult<string>> AnswerQuestionAsync(string question, int? roomId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY")
        {
            return ServiceResult<string>.Failure("Gemini API Key is not configured. Please add it to appsettings.json.", "CONFIG_MISSING");
        }

        try
        {
            string context = "You are a helpful AI concierge for Grand Azure Hotel. ";
            if (roomId.HasValue)
            {
                var roomResult = await _roomService.GetRoomByIdAsync(roomId.Value, ct);
                if (roomResult.IsSuccess)
                {
                    var r = roomResult.Data!;
                    context += $"The user is currently looking at the '{r.Name}' room. " +
                               $"Description: {r.Description}. " +
                               $"Price: ${r.PricePerNight}/night. " +
                               $"Max occupancy: {r.MaxOccupancy} people. " +
                               $"Amenities: {r.Amenities}. ";
                }
            }

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = context + "\n\nUser Question: " + question }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}",
                content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                return ServiceResult<string>.Failure($"AI Service Error: {response.StatusCode}. Details: {error}", "AI_ERROR");
            }

            var responseData = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseData);
            var answer = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return ServiceResult<string>.Success(answer?.Trim() ?? "I'm sorry, I couldn't generate an answer.");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Failure($"AI Service Exception: {ex.Message}", "AI_EXCEPTION");
        }
    }
}
