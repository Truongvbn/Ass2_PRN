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

    public async Task<ServiceResult<AiResponseDto>> AnswerQuestionAsync(string question, int? roomId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY")
        {
            return ServiceResult<AiResponseDto>.Failure("Gemini API Key is not configured. Please add it to appsettings.json.", "CONFIG_MISSING");
        }

        try
        {
            StringBuilder contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("You are a professional and helpful AI concierge for Grand Azure Hotel.");
            contextBuilder.AppendLine("Your goal is to assist guests with their questions and help them find the perfect room.");
            contextBuilder.AppendLine("IMPORTANT: You must output your response in JSON format with the following structure:");
            contextBuilder.AppendLine("{ \"answer\": \"Your markdown-formatted answer here\", \"action\": \"REDIRECT_BOOKING\" (if user wants to book, else null), \"actionData\": \"roomId\" (the ID of the room to book, else null) }");

            // 1. Context about the current room if provided
            if (roomId.HasValue)
            {
                var roomResult = await _roomService.GetRoomByIdAsync(roomId.Value, ct);
                if (roomResult.IsSuccess)
                {
                    var r = roomResult.Data!;
                    contextBuilder.AppendLine($"\n[CURRENT ROOM CONTEXT]");
                    contextBuilder.AppendLine($"The user is currently viewing the '{r.Name}' room (ID: {r.Id}).");
                    contextBuilder.AppendLine($"Description: {r.Description}");
                    contextBuilder.AppendLine($"Price: ${r.PricePerNight}/night");
                    contextBuilder.AppendLine($"Max occupancy: {r.MaxOccupancy} people");
                    contextBuilder.AppendLine($"Amenities: {r.Amenities}");
                }
            }

            // 2. Global context about all available rooms (for agentic recommendations)
            var allRoomsResult = await _roomService.SearchRoomsAsync(null, null, null, null, null, null, null, ct);
            if (allRoomsResult.IsSuccess && allRoomsResult.Data != null)
            {
                contextBuilder.AppendLine("\n[AVAILABLE ROOMS IN HOTEL]");
                foreach (var r in allRoomsResult.Data.Take(10)) // Limit to 10 for context window efficiency
                {
                    contextBuilder.AppendLine($"- Room: {r.Name} (ID: {r.Id}), Type: {r.RoomTypeName}, Price: ${r.PricePerNight}/night, Max Occupancy: {r.MaxOccupancy}");
                }
            }

            contextBuilder.AppendLine("\n[INSTRUCTIONS]");
            contextBuilder.AppendLine("- If the user asks for a recommendation or says they are looking for a room, suggest the best 1-2 options.");
            contextBuilder.AppendLine("- If the user explicitly expresses a desire to book (e.g., 'I want to book this', 'reserve it'), set 'action' to 'REDIRECT_BOOKING' and 'actionData' to the relevant roomId.");
            contextBuilder.AppendLine("- If a specific room is being viewed, favor it in your response unless it doesn't meet the user's requirements.");
            contextBuilder.AppendLine("- Always return ONLY valid JSON.");

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = contextBuilder.ToString() + "\n\nUser Question: " + question }
                        }
                    }
                },
                generationConfig = new { response_mime_type = "application/json" }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}",
                content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                return ServiceResult<AiResponseDto>.Failure($"AI Service Error: {response.StatusCode}. Details: {error}", "AI_ERROR");
            }

            var responseData = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseData);
            var jsonString = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(jsonString))
                return ServiceResult<AiResponseDto>.Failure("Empty response from AI", "AI_EMPTY");

            var aiResponse = JsonSerializer.Deserialize<AiResponseDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return ServiceResult<AiResponseDto>.Success(aiResponse ?? new AiResponseDto { Answer = "I'm sorry, I couldn't process your request." });
        }
        catch (Exception ex)
        {
            return ServiceResult<AiResponseDto>.Failure($"AI Service Exception: {ex.Message}", "AI_EXCEPTION");
        }
    }
}
