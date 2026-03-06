using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController(IAiAssistantService aiService) : ControllerBase
{
    [HttpGet("answer")]
    public async Task<IActionResult> AnswerQuestion([FromQuery] string question, [FromQuery] int? roomId)
    {
        if (string.IsNullOrWhiteSpace(question))
            return BadRequest(new { answer = "Please ask a question." });

        var result = await aiService.AnswerQuestionAsync(question, roomId);
        
        return result.IsSuccess 
            ? Ok(new { answer = result.Data }) 
            : BadRequest(new { answer = result.ErrorMessage });
    }
}
