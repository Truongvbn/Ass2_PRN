using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Account;

public class LogoutModel(IAuthService authService) : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        await authService.LogoutAsync();
        return RedirectToPage("/Index");
    }
}
