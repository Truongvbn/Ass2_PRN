using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Web.Controllers;

[ApiController]
[Route("api")]
public class HotelsApiController(IHotelService hotelService, IRoomService roomService, IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpGet("hotels")]
    public async Task<IActionResult> GetHotels([FromQuery] string? city, CancellationToken ct)
    {
        var result = await hotelService.GetAllHotelsAsync(ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.ErrorMessage });

        var hotels = result.Data!
            .Where(h => h.IsActive)
            .Where(h => string.IsNullOrWhiteSpace(city) || string.Equals(h.City, city, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Ok(hotels);
    }

    [HttpGet("hotels/{id:int}")]
    public async Task<IActionResult> GetHotelById([FromRoute] int id, CancellationToken ct)
    {
        var result = await hotelService.GetHotelByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : NotFound(new { error = result.ErrorMessage });
    }

    [HttpGet("hotels/{id:int}/rooms")]
    public async Task<IActionResult> GetHotelRooms(
        [FromRoute] int id,
        [FromQuery] DateTime? checkIn,
        [FromQuery] DateTime? checkOut,
        [FromQuery] int? guests,
        CancellationToken ct)
    {
        var roomsResult = await roomService.SearchRoomsAsync(id, null, null, null, guests, checkIn, checkOut, ct);
        return roomsResult.IsSuccess ? Ok(roomsResult.Data) : BadRequest(new { error = roomsResult.ErrorMessage });
    }

    // Proxy provinces API to avoid CORS issues and allow caching later.
    [HttpGet("geo/provinces")]
    public async Task<IActionResult> GetProvinces(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("vn-provinces");
        using var res = await client.GetAsync("https://provinces.open-api.vn/api/v2/", ct);
        if (!res.IsSuccessStatusCode)
            return StatusCode((int)res.StatusCode, new { error = "Failed to fetch provinces." });

        var json = await res.Content.ReadAsStringAsync(ct);

        // Return raw JSON as-is (the upstream is already JSON array).
        return Content(json, "application/json");
    }
}

