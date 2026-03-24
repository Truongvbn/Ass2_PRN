using System.Text;
using System.Text.Json;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HotelBooking.Business.Services;

public class OpenAiAssistantService : IAiAssistantService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly IRoomService _roomService;

    public OpenAiAssistantService(HttpClient httpClient, IConfiguration configuration, IRoomService roomService)
    {
        _httpClient = httpClient;
        _apiKey = configuration["AiAssistant:OpenAiApiKey"] ?? "";
        _model = configuration["AiAssistant:OpenAiModel"] ?? "gpt-4o-mini";
        _roomService = roomService;
    }

    public async Task<ServiceResult<IReadOnlyList<RoomListDto>>> RecommendRoomsAsync(RoomPreferenceDto preferences, CancellationToken ct = default)
    {
        var result = await _roomService.SearchRoomsAsync(null, null, null, preferences.MaxBudget, preferences.GuestCount, preferences.CheckIn, preferences.CheckOut, ct);
        if (!result.IsSuccess) return result;

        var rooms = result.Data!;
        return ServiceResult<IReadOnlyList<RoomListDto>>.Success(rooms.Take(5).ToList());
    }

    public async Task<ServiceResult<AiResponseDto>> AnswerQuestionAsync(string question, int? roomId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "OPENAI_API_KEY")
        {
            return ServiceResult<AiResponseDto>.Failure("OpenAI API Key is not configured. Please add it to appsettings.json.", "CONFIG_MISSING");
        }

        try
        {
            var systemPrompt = await BuildSystemPromptAsync(roomId, ct);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var requestBody = new
            {
                model = _model,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = question }
                },
                response_format = new { type = "json_object" },
                temperature = 0.4
            };

            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            var responseText = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<AiResponseDto>.Failure($"AI Service Error: {response.StatusCode}. Details: {responseText}", "AI_ERROR");
            }

            using var doc = JsonDocument.Parse(responseText);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
                return ServiceResult<AiResponseDto>.Failure("Empty response from AI", "AI_EMPTY");

            var aiResponse = JsonSerializer.Deserialize<AiResponseDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (aiResponse is null || string.IsNullOrWhiteSpace(aiResponse.Answer))
                return ServiceResult<AiResponseDto>.Failure("Invalid AI response format", "AI_INVALID");

            aiResponse.Answer = aiResponse.Answer.Trim();
            aiResponse.Action = string.IsNullOrWhiteSpace(aiResponse.Action) ? null : aiResponse.Action.Trim();
            aiResponse.ActionData = string.IsNullOrWhiteSpace(aiResponse.ActionData) ? null : aiResponse.ActionData.Trim();

            return ServiceResult<AiResponseDto>.Success(aiResponse);
        }
        catch (Exception ex)
        {
            return ServiceResult<AiResponseDto>.Failure($"AI Service Exception: {ex.Message}", "AI_EXCEPTION");
        }
    }

    private async Task<string> BuildSystemPromptAsync(int? roomId, CancellationToken ct)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("You are a professional and helpful AI concierge for Grand Azure Hotel.");
        sb.AppendLine("Use Markdown in your answer so it is easy to read.");
        sb.AppendLine("You must output ONLY valid JSON with this exact structure:");
        sb.AppendLine("{\"answer\":\"markdown\",\"action\":null|\"REDIRECT_BOOKING\",\"actionData\":null|\"roomId\"}");
        sb.AppendLine("Set action to REDIRECT_BOOKING only if the user explicitly wants to book or reserve.");

        if (roomId.HasValue)
        {
            var roomResult = await _roomService.GetRoomByIdAsync(roomId.Value, ct);
            if (roomResult.IsSuccess)
            {
                var r = roomResult.Data!;
                sb.AppendLine();
                sb.AppendLine("[CURRENT ROOM CONTEXT]");
                sb.AppendLine($"Room ID: {r.Id}");
                sb.AppendLine($"Name: {r.Name}");
                sb.AppendLine($"Description: {r.Description}");
                sb.AppendLine($"Price per night: {r.PricePerNight}");
                sb.AppendLine($"Max occupancy: {r.MaxOccupancy}");
                sb.AppendLine($"Amenities: {r.Amenities}");
            }
        }

        var allRoomsResult = await _roomService.SearchRoomsAsync(null, null, null, null, null, null, null, ct);
        if (allRoomsResult.IsSuccess && allRoomsResult.Data != null)
        {
            sb.AppendLine();
            sb.AppendLine("[AVAILABLE ROOMS]");
            foreach (var r in allRoomsResult.Data.Take(10))
            {
                sb.AppendLine($"- ID {r.Id}: {r.Name}, Type: {r.RoomTypeName}, Price: {r.PricePerNight}, Max: {r.MaxOccupancy}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("If the user asks for recommendations, suggest the best 1-2 rooms with IDs and why.");

        return sb.ToString();
    }
}

