using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Pages.Admin.Hotels;

[Authorize(Roles = "Admin")]
public class CreateModel(IHotelService hotelService) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required] public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        [Range(1, 5)] public int StarRating { get; set; } = 3;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var dto = new CreateHotelDto
        {
            Name = Input.Name,
            Description = Input.Description,
            Address = Input.Address,
            City = Input.City,
            Latitude = Input.Latitude,
            Longitude = Input.Longitude,
            PhoneNumber = Input.PhoneNumber,
            Email = Input.Email,
            ImageUrl = Input.ImageUrl,
            StarRating = Input.StarRating
        };

        var result = await hotelService.CreateHotelAsync(dto);
        if (result.IsSuccess) return RedirectToPage("/Admin/Hotels/Index");

        ErrorMessage = result.ErrorMessage;
        return Page();
    }
}

