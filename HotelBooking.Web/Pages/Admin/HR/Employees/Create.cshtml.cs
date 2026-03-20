using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Employees;

[Authorize(Roles = "Admin,Staff")]
public class CreateModel(IEmployeeService employeeService, IHotelService hotelService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public IReadOnlyList<ApplicationUser> StaffUsers { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required] public string UserId { get; set; } = "";
        public int? HotelId { get; set; }
        [Required] public string FullName { get; set; } = "";
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.Date;
        public string Gender { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string IdentityNumber { get; set; } = "";
        public string Position { get; set; } = "";
        public DateTime HireDate { get; set; } = DateTime.UtcNow.Date;
        public string EmploymentType { get; set; } = "";
        public decimal BaseSalary { get; set; }
    }

    public async Task OnGetAsync()
    {
        Hotels = await LoadHotelsAsync();
        StaffUsers = (await userManager.GetUsersInRoleAsync("Staff")).ToList();

        if (!Input.HotelId.HasValue || Input.HotelId.Value == 0)
        {
            Input.HotelId = Hotels.FirstOrDefault()?.Id;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Hotels = await LoadHotelsAsync();
        if (!ModelState.IsValid) return Page();

        if (!User.IsInRole("Admin"))
        {
            if (!Input.HotelId.HasValue || !Hotels.Any(h => h.Id == Input.HotelId.Value))
                return Forbid();
        }

        var dto = new CreateEmployeeDto
        {
            UserId = Input.UserId,
            HotelId = Input.HotelId,
            FullName = Input.FullName,
            DateOfBirth = Input.DateOfBirth,
            Gender = Input.Gender,
            PhoneNumber = Input.PhoneNumber,
            Email = Input.Email,
            Address = Input.Address,
            IdentityNumber = Input.IdentityNumber,
            Position = Input.Position,
            HireDate = Input.HireDate,
            EmploymentType = Input.EmploymentType,
            BaseSalary = Input.BaseSalary
        };

        var result = await employeeService.CreateEmployeeAsync(dto);
        if (result.IsSuccess) return RedirectToPage("/Admin/HR/Employees/Index");

        ErrorMessage = result.ErrorMessage;
        return Page();
    }

    private async Task<IReadOnlyList<HotelDto>> LoadHotelsAsync()
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

