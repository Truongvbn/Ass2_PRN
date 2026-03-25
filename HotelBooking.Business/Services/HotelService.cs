using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HotelBooking.Business.Services;

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepo;
    private readonly IHotelStaffRepository _hotelStaffRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IBookingHubNotifier _notifier;

    public HotelService(
        IHotelRepository hotelRepo,
        IHotelStaffRepository hotelStaffRepo,
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IBookingHubNotifier notifier)
    {
        _hotelRepo = hotelRepo;
        _hotelStaffRepo = hotelStaffRepo;
        _userManager = userManager;
        _mapper = mapper;
        _notifier = notifier;
    }

    public async Task<ServiceResult<IReadOnlyList<HotelDto>>> GetAllHotelsAsync(CancellationToken ct = default)
    {
        var hotels = await _hotelRepo.GetAllAsync(ct);
        return ServiceResult<IReadOnlyList<HotelDto>>.Success(_mapper.Map<IReadOnlyList<HotelDto>>(hotels));
    }

    public async Task<ServiceResult<HotelDto>> GetHotelByIdAsync(int id, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetWithRoomsAsync(id, ct);
        if (hotel is null) return ServiceResult<HotelDto>.Failure("Hotel not found", "NOT_FOUND");
        return ServiceResult<HotelDto>.Success(_mapper.Map<HotelDto>(hotel));
    }

    public async Task<ServiceResult<IReadOnlyList<HotelDto>>> GetHotelsByStaffAsync(string userId, CancellationToken ct = default)
    {
        var hotels = await _hotelRepo.GetByStaffUserAsync(userId, ct);
        return ServiceResult<IReadOnlyList<HotelDto>>.Success(_mapper.Map<IReadOnlyList<HotelDto>>(hotels));
    }

    public async Task<ServiceResult<HotelDto>> CreateHotelAsync(CreateHotelDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ServiceResult<HotelDto>.Failure("Hotel name is required", "VALIDATION");

        var hotel = _mapper.Map<Hotel>(dto);
        hotel.CreatedAt = DateTime.UtcNow;
        hotel.IsActive = true;
        await _hotelRepo.AddAsync(hotel, ct);

        var created = await _hotelRepo.GetWithRoomsAsync(hotel.Id, ct);
        return ServiceResult<HotelDto>.Success(_mapper.Map<HotelDto>(created!));
    }

    public async Task<ServiceResult<HotelDto>> UpdateHotelAsync(UpdateHotelDto dto, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetByIdAsync(dto.Id, ct);
        if (hotel is null) return ServiceResult<HotelDto>.Failure("Hotel not found", "NOT_FOUND");

        hotel.Name = dto.Name;
        hotel.Description = dto.Description;
        hotel.Address = dto.Address;
        hotel.City = dto.City;
        hotel.PhoneNumber = dto.PhoneNumber;
        hotel.Email = dto.Email;
        hotel.ImageUrl = dto.ImageUrl;
        hotel.StarRating = dto.StarRating;
        hotel.IsActive = dto.IsActive;

        await _hotelRepo.UpdateAsync(hotel, ct);
        var updated = await _hotelRepo.GetWithRoomsAsync(hotel.Id, ct);
        return ServiceResult<HotelDto>.Success(_mapper.Map<HotelDto>(updated!));
    }

    public async Task<ServiceResult> DeleteHotelAsync(int id, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetByIdAsync(id, ct);
        if (hotel is null) return ServiceResult.Failure("Hotel not found", "NOT_FOUND");

        await _hotelRepo.DeleteAsync(hotel, ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyList<HotelStaffDto>>> GetHotelStaffAsync(int hotelId, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetByIdAsync(hotelId, ct);
        if (hotel is null) return ServiceResult<IReadOnlyList<HotelStaffDto>>.Failure("Hotel not found", "NOT_FOUND");

        var staff = await _hotelStaffRepo.GetByHotelAsync(hotelId, ct);
        return ServiceResult<IReadOnlyList<HotelStaffDto>>.Success(_mapper.Map<IReadOnlyList<HotelStaffDto>>(staff));
    }

    public async Task<ServiceResult> AssignStaffAsync(AssignStaffDto dto, CancellationToken ct = default)
    {
        if (dto.HotelId <= 0) return ServiceResult.Failure("Hotel is required", "VALIDATION");
        if (string.IsNullOrWhiteSpace(dto.UserId)) return ServiceResult.Failure("User is required", "VALIDATION");

        var hotel = await _hotelRepo.GetByIdAsync(dto.HotelId, ct);
        if (hotel is null) return ServiceResult.Failure("Hotel not found", "NOT_FOUND");

        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null) return ServiceResult.Failure("User not found", "NOT_FOUND");

        // Ensure the user has the Staff role; remove Customer role if present
        var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
        if (!isStaff)
        {
            var addRes = await _userManager.AddToRoleAsync(user, "Staff");
            if (!addRes.Succeeded) return ServiceResult.Failure("Failed to add Staff role", "IDENTITY");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            if (isCustomer) await _userManager.RemoveFromRoleAsync(user, "Customer");
        }

        var existing = await _hotelStaffRepo.GetAssignmentAsync(dto.HotelId, dto.UserId, ct);
        if (existing is not null) return ServiceResult.Failure("User is already assigned to this hotel", "DUPLICATE");

        if (!Enum.TryParse<HotelStaffRole>(dto.Role, ignoreCase: true, out var role))
            role = HotelStaffRole.Receptionist;

        await _hotelStaffRepo.AddAsync(new HotelStaff
        {
            HotelId = dto.HotelId,
            UserId = dto.UserId,
            Role = role,
            AssignedAt = DateTime.UtcNow
        }, ct);

        await _notifier.RolePromoted(dto.UserId);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveStaffAsync(int hotelId, string userId, CancellationToken ct = default)
    {
        var assignment = await _hotelStaffRepo.GetAssignmentAsync(hotelId, userId, ct);
        if (assignment is null) return ServiceResult.Failure("Assignment not found", "NOT_FOUND");

        await _hotelStaffRepo.DeleteAsync(assignment, ct);
        return ServiceResult.Success();
    }
}

