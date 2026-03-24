using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Business.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HotelBooking.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController(IAiAssistantService aiService, IBookingService bookingService, IRoomService roomService) : ControllerBase
{
    [HttpGet("answer")]
    public async Task<IActionResult> AnswerQuestion([FromQuery] string question, [FromQuery] int? roomId)
    {
         if (string.IsNullOrWhiteSpace(question))
            return BadRequest(new AiResponseDto { Answer = "Please ask a question." });

        var result = await aiService.AnswerQuestionAsync(question, roomId);
        
        return result.IsSuccess 
            ? Ok(result.Data) 
            : BadRequest(new AiResponseDto { Answer = result.ErrorMessage ?? "AI error" });

    }

    [Authorize]
    [HttpPost("quick-book")]
    public async Task<IActionResult> QuickBook([FromForm] int roomId, [FromForm] DateTime? checkIn, [FromForm] DateTime? checkOut, [FromForm] int? guests)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { error = "Login required" });

        var roomResult = await roomService.GetRoomByIdAsync(roomId);
        if (!roomResult.IsSuccess || roomResult.Data is null)
            return BadRequest(new { error = "Room not found" });

        var ci = (checkIn ?? DateTime.UtcNow.Date.AddDays(1)).Date;
        var co = (checkOut ?? ci.AddDays(1)).Date;
        var g = guests ?? Math.Min(1, roomResult.Data.MaxOccupancy);

        var createDto = new CreateBookingDto
        {
            RoomId = roomId,
            CheckIn = ci,
            CheckOut = co,
            NumberOfGuests = g,
            GuestNotes = "Created via AI concierge quick-book"
        };

        var createResult = await bookingService.CreateBookingAsync(createDto, userId);
        if (!createResult.IsSuccess || createResult.Data is null)
            return BadRequest(new { error = createResult.ErrorMessage ?? "Failed to create booking" });

        // Move booking to AwaitingPayment to allow payment page access
        var approveResult = await bookingService.ApproveBookingAsync(createResult.Data.Id);
        if (!approveResult.IsSuccess)
            return BadRequest(new { error = approveResult.ErrorMessage ?? "Failed to prepare payment" });

        var paymentUrl = $"/Booking/Payment?id={createResult.Data.Id}";
        return Ok(new { bookingId = createResult.Data.Id, paymentUrl });
    }
}
