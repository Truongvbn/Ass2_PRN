using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Data.Repositories;

public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Room>> SearchAsync(
        int? roomTypeId, decimal? minPrice, decimal? maxPrice,
        int? minOccupancy, DateTime? checkIn, DateTime? checkOut,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().Include(r => r.RoomType).AsQueryable();

        if (roomTypeId.HasValue)
            query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
        if (minPrice.HasValue)
            query = query.Where(r => r.PricePerNight >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(r => r.PricePerNight <= maxPrice.Value);
        if (minOccupancy.HasValue)
            query = query.Where(r => r.MaxOccupancy >= minOccupancy.Value);
        if (checkIn.HasValue && checkOut.HasValue)
        {
            // Exclude rooms with overlapping confirmed/pending bookings
            query = query.Where(r => !r.Bookings.Any(b =>
                b.Status != BookingStatus.Cancelled &&
                b.CheckIn < checkOut.Value &&
                b.CheckOut > checkIn.Value));
        }

        return await query.Where(r => r.IsAvailable).OrderBy(r => r.PricePerNight).ToListAsync(ct);
    }

    public async Task<Room?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => await DbSet.Include(r => r.RoomType)
            .Include(r => r.Reviews.Where(rv => !rv.IsDeleted))
                .ThenInclude(rv => rv.User)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
}

public class RoomTypeRepository : Repository<RoomType>, IRoomTypeRepository
{
    public RoomTypeRepository(HotelDbContext context) : base(context) { }
}

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(HotelDbContext context) : base(context) { }

    public async Task<bool> HasOverlappingBookingAsync(int roomId, DateTime checkIn, DateTime checkOut,
        int? excludeBookingId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(b =>
            b.RoomId == roomId &&
            b.Status != BookingStatus.Cancelled &&
            b.CheckIn < checkOut &&
            b.CheckOut > checkIn);

        if (excludeBookingId.HasValue)
            query = query.Where(b => b.Id != excludeBookingId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<Booking>> GetByUserAsync(string userId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(b => b.Payment)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public async Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => await DbSet.Include(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(b => b.User)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
}

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Review>> GetByRoomWithCommentsAsync(int roomId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.User)
            .Where(r => r.RoomId == roomId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<double> GetAverageRatingAsync(int roomId, CancellationToken ct = default)
    {
        var ratings = await DbSet.Where(r => r.RoomId == roomId).Select(r => r.Rating).ToListAsync(ct);
        return ratings.Count != 0 ? ratings.Average() : 0;
    }

    public async Task<bool> HasUserReviewedRoomAsync(string userId, int roomId, CancellationToken ct = default)
        => await DbSet.AnyAsync(r => r.UserId == userId && r.RoomId == roomId && !r.IsDeleted, ct);
}

public class ReviewCommentRepository : Repository<ReviewComment>, IReviewCommentRepository
{
    public ReviewCommentRepository(HotelDbContext context) : base(context) { }
}

public class TicketRepository : Repository<SupportTicket>, ITicketRepository
{
    public TicketRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<SupportTicket>> GetByUserAsync(string userId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(t => t.AssignedTo)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SupportTicket>> GetActiveTicketsAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.AssignedTo)
            .Where(t => t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SupportTicket>> GetAssignedToAsync(string staffId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(t => t.User)
            .Where(t => t.AssignedToId == staffId && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
}

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(HotelDbContext context) : base(context) { }

    public async Task<Payment?> GetByBookingIdAsync(int bookingId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(p => p.BookingId == bookingId, ct);
}
