using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Hotels;

[Authorize(Roles = "Admin")]
public class StaffModel(IHotelService hotelService, UserManager<ApplicationUser> userManager) : PageModel
{
    public HotelDto? Hotel { get; set; }
    public IReadOnlyList<HotelStaffDto> Staff { get; set; } = [];
    public IReadOnlyList<ApplicationUser> StaffUsers { get; set; } = [];

    public string? Message { get; set; }
    public bool IsError { get; set; }

    [BindProperty] public AssignInput Input { get; set; } = new();

    public class AssignInput
    {
        public int HotelId { get; set; }
        public string UserId { get; set; } = "";
        public string Role { get; set; } = "Receptionist";
    }

    public async Task OnGetAsync(int id)
    {
        Input.HotelId = id;
        await LoadAsync(id);
    }

    public async Task<IActionResult> OnPostAssignAsync()
    {
        await LoadAsync(Input.HotelId);

        var result = await hotelService.AssignStaffAsync(new AssignStaffDto
        {
            HotelId = Input.HotelId,
            UserId = Input.UserId,
            Role = Input.Role
        });

        Message = result.IsSuccess ? "Staff assigned." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await LoadAsync(Input.HotelId);
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveAsync(int hotelId, string userId)
    {
        await LoadAsync(hotelId);

        var result = await hotelService.RemoveStaffAsync(hotelId, userId);
        Message = result.IsSuccess ? "Staff removed." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await LoadAsync(hotelId);
        return Page();
    }

    [BindProperty] public PromoteInput Promote { get; set; } = new();
    public class PromoteInput
    {
        public int HotelId { get; set; }
        public string Email { get; set; } = "";
        public bool AssignNow { get; set; } = true;
        public string Role { get; set; } = "Receptionist";
    }

    public async Task<IActionResult> OnPostPromoteAsync()
    {
        await LoadAsync(Promote.HotelId);
        if (string.IsNullOrWhiteSpace(Promote.Email))
        {
            Message = "Email is required";
            IsError = true;
            return Page();
        }
        var user = await userManager.FindByEmailAsync(Promote.Email);
        if (user is null)
        {
            Message = "User not found";
            IsError = true;
            return Page();
        }
        if (Promote.AssignNow)
        {
            var assignRes = await hotelService.AssignStaffAsync(new AssignStaffDto
            {
                HotelId = Promote.HotelId,
                UserId = user.Id,
                Role = Promote.Role
            });
            if (!assignRes.IsSuccess)
            {
                Message = assignRes.ErrorMessage ?? "Failed to promote and assign staff.";
                IsError = true;
                return Page();
            }
            Message = "User promoted and assigned to hotel.";
            IsError = false;
            await LoadAsync(Promote.HotelId);
            return Page();
        }
        else
        {
            var isStaff = await userManager.IsInRoleAsync(user, "Staff");
            if (!isStaff)
            {
                var addRes = await userManager.AddToRoleAsync(user, "Staff");
                if (!addRes.Succeeded)
                {
                    Message = "Failed to add Staff role";
                    IsError = true;
                    return Page();
                }
                var isCustomer = await userManager.IsInRoleAsync(user, "Customer");
                if (isCustomer) await userManager.RemoveFromRoleAsync(user, "Customer");
            }
            Message = "User promoted to Staff.";
            IsError = false;
        }
        await LoadAsync(Promote.HotelId);
        return Page();
    }

    private async Task LoadAsync(int hotelId)
    {
        var hotelResult = await hotelService.GetHotelByIdAsync(hotelId);
        if (hotelResult.IsSuccess) Hotel = hotelResult.Data;

        var staffResult = await hotelService.GetHotelStaffAsync(hotelId);
        if (staffResult.IsSuccess && staffResult.Data is not null) Staff = staffResult.Data;

        StaffUsers = (await userManager.GetUsersInRoleAsync("Staff")).ToList();
    }
}

