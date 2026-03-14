using HotelBooking.Data.Entities;

namespace HotelBooking.Data.Repositories.Interfaces;

public interface IRoomRepository : IRepository<Room>
{
    Task<IReadOnlyList<Room>> SearchAsync(
        int? roomTypeId, decimal? minPrice, decimal? maxPrice,
        int? minOccupancy, DateTime? checkIn, DateTime? checkOut,
        CancellationToken ct = default);

    Task<IReadOnlyList<Room>> SearchByHotelAsync(
        int hotelId,
        int? roomTypeId, decimal? minPrice, decimal? maxPrice,
        int? minOccupancy, DateTime? checkIn, DateTime? checkOut,
        CancellationToken ct = default);

    Task<Room?> GetWithDetailsAsync(int id, CancellationToken ct = default);
}

public interface IRoomTypeRepository : IRepository<RoomType>
{
}

public interface IBookingRepository : IRepository<Booking>
{
    Task<bool> HasOverlappingBookingAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetByHotelAsync(int hotelId, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetExpiredPendingAsync(DateTime utcNow, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetExpiredAwaitingPaymentAsync(DateTime utcNow, CancellationToken ct = default);
}

public interface IHotelRepository : IRepository<Hotel>
{
    Task<IReadOnlyList<Hotel>> GetActiveHotelsAsync(CancellationToken ct = default);
    Task<Hotel?> GetWithRoomsAsync(int hotelId, CancellationToken ct = default);
    Task<IReadOnlyList<Hotel>> GetByStaffUserAsync(string userId, CancellationToken ct = default);
}

public interface IHotelStaffRepository : IRepository<HotelStaff>
{
    Task<IReadOnlyList<HotelStaff>> GetByHotelAsync(int hotelId, CancellationToken ct = default);
    Task<HotelStaff?> GetAssignmentAsync(int hotelId, string userId, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetHotelIdsByUserAsync(string userId, CancellationToken ct = default);
}

public interface IReviewRepository : IRepository<Review>
{
    Task<IReadOnlyList<Review>> GetByRoomWithCommentsAsync(int roomId, CancellationToken ct = default);
    Task<double> GetAverageRatingAsync(int roomId, CancellationToken ct = default);
    Task<bool> HasUserReviewedRoomAsync(string userId, int roomId, CancellationToken ct = default);
}

public interface IReviewCommentRepository : IRepository<ReviewComment>
{
}

public interface ITicketRepository : IRepository<SupportTicket>
{
    Task<IReadOnlyList<SupportTicket>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<SupportTicket>> GetActiveTicketsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SupportTicket>> GetAssignedToAsync(string staffId, CancellationToken ct = default);
}

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByBookingIdAsync(int bookingId, CancellationToken ct = default);
}
