using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Training.Programs;

[Authorize(Roles = "Admin,Staff")]
public class EditModel(ITrainingService trainingService, IHotelService hotelService) : PageModel
{
    public TrainingProgramDto? Program { get; set; }
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public int Id { get; set; }

        // 0 = Global
        public int HotelId { get; set; }

        [Required] public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.Date;
        public bool IsMandatory { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var res = await trainingService.GetTrainingProgramByIdAsync(id);
        if (!res.IsSuccess || res.Data is null)
            return NotFound();

        Program = res.Data;
        if (!await CanAccessProgramAsync(Program))
            return Forbid();

        Hotels = await GetScopedHotelsAsync();

        Input = new InputModel
        {
            Id = Program.Id,
            HotelId = Program.HotelId ?? 0,
            Title = Program.Title,
            Description = Program.Description,
            StartDate = Program.StartDate,
            EndDate = Program.EndDate,
            IsMandatory = Program.IsMandatory
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        if (!ModelState.IsValid) return Page();

        var res = await trainingService.GetTrainingProgramByIdAsync(Input.Id);
        if (!res.IsSuccess || res.Data is null)
            return NotFound();

        if (!await CanAccessProgramAsync(res.Data))
            return Forbid();

        if (Input.EndDate.Date <= Input.StartDate.Date)
        {
            ErrorMessage = "EndDate must be after StartDate.";
            return Page();
        }

        if (!User.IsInRole("Admin") && Input.HotelId == 0)
        {
            ErrorMessage = "Staff cannot move training programs to Global.";
            return Page();
        }

        var dto = new UpdateTrainingProgramDto
        {
            Id = Input.Id,
            HotelId = Input.HotelId == 0 ? null : Input.HotelId,
            Title = Input.Title,
            Description = Input.Description,
            StartDate = Input.StartDate.Date,
            EndDate = Input.EndDate.Date,
            IsMandatory = Input.IsMandatory
        };

        var updateRes = await trainingService.UpdateTrainingProgramAsync(dto);
        if (!updateRes.IsSuccess)
        {
            ErrorMessage = updateRes.ErrorMessage;
            return Page();
        }

        return RedirectToPage("/Admin/HR/Training/Programs", new { hotelId = updateRes.Data?.HotelId ?? Input.HotelId });
    }

    private async Task<bool> CanAccessProgramAsync(TrainingProgramDto program)
    {
        if (User.IsInRole("Admin")) return true;
        if (!program.HotelId.HasValue) return false;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;
        var scoped = await hotelService.GetHotelsByStaffAsync(userId);
        return scoped.IsSuccess && scoped.Data is not null && scoped.Data.Any(h => h.Id == program.HotelId.Value);
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

