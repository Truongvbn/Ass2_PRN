using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace HotelBooking.Business.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<ServiceResult> LoginAsync(string email, string password, bool rememberMe)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        return result.Succeeded 
            ? ServiceResult.Success() 
            : ServiceResult.Failure("Invalid email or password.", "LOGIN_FAILED");
    }

    public async Task<ServiceResult> RegisterAsync(string email, string password, string fullName)
    {
        // Require unique email
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return ServiceResult.Failure("Email is already registered.", "DUPLICATE_EMAIL");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return ServiceResult.Success();
        }

        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
        return ServiceResult.Failure(errors, "REGISTRATION_FAILED");
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
