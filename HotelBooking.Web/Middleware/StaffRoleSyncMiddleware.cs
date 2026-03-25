using System.Security.Claims;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace HotelBooking.Web.Middleware;

public class StaffRoleSyncMiddleware
{
    private readonly RequestDelegate _next;

    public StaffRoleSyncMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IHotelService hotelService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var hasStaffRoleInDb = await userManager.IsInRoleAsync(user, "Staff");
                    var hasStaffClaim = context.User.IsInRole("Staff");
                    if (hasStaffRoleInDb && !hasStaffClaim)
                    {
                        await signInManager.RefreshSignInAsync(user);
                    }
                }
            }
        }

        await _next(context);
    }
}
