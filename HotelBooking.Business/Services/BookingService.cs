using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IMapper _mapper;
    private readonly IBookingHubNotifier _notifier;

    public BookingService(IBookingRepository bookingRepo, IRoomRepository roomRepo, IMapper mapper, IBookingHubNotifier notifier)
    {
        _bookingRepo = bookingRepo;
        _roomRepo = roomRepo;
        _mapper = mapper;
        _notifier = notifier;
    }

    public async Task<ServiceResult<BookingDto>> CreateBookingAsync(CreateBookingDto dto, string userId, CancellationToken ct = default)
    {
        // Validate dates
        if (dto.CheckIn.Date < DateTime.Today)
            return ServiceResult<BookingDto>.Failure("Check-in date cannot be in the past", "INVALID_DATE");
        if (dto.CheckOut <= dto.CheckIn)
            return ServiceResult<BookingDto>.Failure("Check-out must be after check-in", "INVALID_DATE");
        if (dto.NumberOfGuests <= 0)
            return ServiceResult<BookingDto>.Failure("Number of guests must be greater than 0", "VALIDATION");

        // Validate room
        var room = await _roomRepo.GetByIdAsync(dto.RoomId, ct);
        if (room is null || room.IsDeleted || !room.IsAvailable)
            return ServiceResult<BookingDto>.Failure("Room is not available", "ROOM_UNAVAILABLE");
        if (dto.NumberOfGuests > room.MaxOccupancy)
            return ServiceResult<BookingDto>.Failure($"Room max occupancy is {room.MaxOccupancy}", "EXCEEDS_OCCUPANCY");

        // Check overlapping bookings
        if (await _bookingRepo.HasOverlappingBookingAsync(dto.RoomId, dto.CheckIn, dto.CheckOut, ct: ct))
            return ServiceResult<BookingDto>.Failure("Room is already booked for these dates", "BOOKING_OVERLAP");

        // Calculate price (server-side, never trust client)
        var nights = (dto.CheckOut.Date - dto.CheckIn.Date).Days;
        var totalPrice = room.PricePerNight * nights;

        var booking = new Booking
        {
            RoomId = dto.RoomId,
            UserId = userId,
            CheckIn = dto.CheckIn.Date,
            CheckOut = dto.CheckOut.Date,
            NumberOfGuests = dto.NumberOfGuests,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _bookingRepo.AddAsync(booking, ct);
        var created = await _bookingRepo.GetWithDetailsAsync(booking.Id, ct);
        var bookingDto = _mapper.Map<BookingDto>(created!);

        await _notifier.BookingCreated(bookingDto);

        return ServiceResult<BookingDto>.Success(bookingDto);
    }

    public async Task<ServiceResult<BookingDto>> GetBookingByIdAsync(int id, string? userId = null, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetWithDetailsAsync(id, ct);
        if (booking is null) return ServiceResult<BookingDto>.Failure("Booking not found", "NOT_FOUND");
        
        if (userId is null)
        {
            // Admin/Staff fetching (no constraint applied)
        }
        else if (booking.UserId != userId)
        {
            return ServiceResult<BookingDto>.Failure("You do not have permission to view this booking", "FORBIDDEN");
        }

        return ServiceResult<BookingDto>.Success(_mapper.Map<BookingDto>(booking));
    }

    public async Task<ServiceResult<IReadOnlyList<BookingDto>>> GetUserBookingsAsync(string userId, CancellationToken ct = default)
    {
        var bookings = await _bookingRepo.GetByUserAsync(userId, ct);
        return ServiceResult<IReadOnlyList<BookingDto>>.Success(_mapper.Map<IReadOnlyList<BookingDto>>(bookings));
    }

    public async Task<ServiceResult<IReadOnlyList<BookingDto>>> GetAllBookingsAsync(CancellationToken ct = default)
    {
        var bookings = await _bookingRepo.GetAllAsync(ct);
        return ServiceResult<IReadOnlyList<BookingDto>>.Success(_mapper.Map<IReadOnlyList<BookingDto>>(bookings));
    }

    public async Task<ServiceResult> ConfirmBookingAsync(int id, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetByIdAsync(id, ct);
        if (booking is null) return ServiceResult.Failure("Booking not found", "NOT_FOUND");
        if (booking.Status != BookingStatus.Pending)
            return ServiceResult.Failure("Only pending bookings can be confirmed", "INVALID_STATE");

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepo.UpdateAsync(booking, ct);
        
        await _notifier.BookingStatusChanged(id, BookingStatus.Confirmed.ToString());
        
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> CancelBookingAsync(int id, string userId, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetByIdAsync(id, ct);
        if (booking is null) return ServiceResult.Failure("Booking not found", "NOT_FOUND");
        if (booking.UserId != userId)
            return ServiceResult.Failure("You can only cancel your own bookings", "FORBIDDEN");
        
        if (booking.Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            return ServiceResult.Failure("Only pending or confirmed bookings can be cancelled", "INVALID_STATE");

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepo.UpdateAsync(booking, ct);

        await _notifier.BookingCancelled(id);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> CompleteBookingAsync(int id, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetByIdAsync(id, ct);
        if (booking is null) return ServiceResult.Failure("Booking not found", "NOT_FOUND");
        if (booking.Status != BookingStatus.Confirmed)
            return ServiceResult.Failure("Only confirmed bookings can be completed", "INVALID_STATE");

        booking.Status = BookingStatus.Completed;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepo.UpdateAsync(booking, ct);

        await _notifier.BookingStatusChanged(id, BookingStatus.Completed.ToString());

        return ServiceResult.Success();
    }
}
