using HotelBooking.Business.DTOs;

namespace HotelBooking.Business.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult> LoginAsync(string email, string password, bool rememberMe);
    Task<ServiceResult> RegisterAsync(string email, string password, string fullName);
    Task LogoutAsync();
}
