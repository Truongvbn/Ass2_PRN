using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Training;

[Authorize(Roles = "Admin,Staff")]
public class ProgramsModel(ITrainingService trainingService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public IReadOnlyList<TrainingProgramDto> Programs { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int? HotelId { get; set; }

    [BindProperty] public CreateProgramInput Input { get; set; } = new();

    public string? Message { get; set; }
    public bool IsError { get; set; }

    public class CreateProgramInput
    {
        // 0 = Global
        public int HotelId { get; set; }

        [Required] public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddDays(7);
        public bool IsMandatory { get; set; }
    }

    public async Task OnGetAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        var targetHotelId = HotelId ?? Hotels.FirstOrDefault()?.Id;
        if (!targetHotelId.HasValue) return;

        HotelId = targetHotelId.Value;
        Input.HotelId = targetHotelId.Value;

        var result = await trainingService.GetTrainingProgramsAsync(targetHotelId.Value);
        if (result.IsSuccess && result.Data is not null)
            Programs = result.Data;
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        var targetHotelId = HotelId ?? Hotels.FirstOrDefault()?.Id;
        if (!targetHotelId.HasValue) return Forbid();

        Input.HotelId = Input.HotelId == 0 ? 0 : Input.HotelId;

        if (!User.IsInRole("Admin") && Input.HotelId == 0)
        {
            IsError = true;
            Message = "Staff cannot create global training programs.";
            return Page();
        }

        if (!User.IsInRole("Admin") && !Hotels.Any(h => h.Id == Input.HotelId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
            return Page();

        if (Input.EndDate.Date <= Input.StartDate.Date)
        {
            IsError = true;
            Message = "EndDate must be after StartDate.";
            return Page();
        }

        var dto = new CreateTrainingProgramDto
        {
            HotelId = Input.HotelId == 0 ? null : Input.HotelId,
            Title = Input.Title,
            Description = Input.Description,
            StartDate = Input.StartDate.Date,
            EndDate = Input.EndDate.Date,
            IsMandatory = Input.IsMandatory
        };

        var result = await trainingService.CreateTrainingProgramAsync(dto);
        Message = result.IsSuccess ? "Training program created." : result.ErrorMessage;
        IsError = !result.IsSuccess;

        return RedirectToPage("/Admin/HR/Training/Programs", new { hotelId = targetHotelId.Value });
    }

    private async Task<IReadOnlyList<HotelDto>> GetScopedHotelsAsync()
    {
        if (User.IsInRole("Admin"))
        {
            var all = await hotelService.GetAllHotelsAsync();
            return all.IsSuccess && all.Data is not null ? all.Data : [];
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return [];

        var scoped = await hotelService.GetHotelsByStaffAsync(userId);
        return scoped.IsSuccess && scoped.Data is not null ? scoped.Data : [];
    }
}

