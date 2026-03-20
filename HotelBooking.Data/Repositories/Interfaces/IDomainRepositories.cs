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

// ── HR: Employee & Scheduling ──
public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IReadOnlyList<Employee>> GetByHotelAsync(int hotelId, CancellationToken ct = default);
    Task<Employee?> GetByUserIdAsync(string userId, CancellationToken ct = default);
}

public interface IWorkShiftRepository : IRepository<WorkShift>
{
    Task<IReadOnlyList<WorkShift>> GetByHotelAsync(int hotelId, CancellationToken ct = default);
}

public interface IEmployeeShiftAssignmentRepository : IRepository<EmployeeShiftAssignment>
{
    Task<IReadOnlyList<EmployeeShiftAssignment>> GetByHotelAndDateRangeAsync(int hotelId, DateTime start, DateTime end, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeShiftAssignment>> GetByEmployeeAndDateRangeAsync(int employeeId, DateTime start, DateTime end, CancellationToken ct = default);
}

public interface IAttendanceRepository : IRepository<AttendanceRecord>
{
    Task<IReadOnlyList<AttendanceRecord>> GetByHotelAndDateRangeAsync(int hotelId, DateTime start, DateTime end, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceRecord>> GetByEmployeeAndDateRangeAsync(int employeeId, DateTime start, DateTime end, CancellationToken ct = default);
}

// ── HR: Payroll ──
public interface IPayrollPeriodRepository : IRepository<PayrollPeriod>
{
    Task<IReadOnlyList<PayrollPeriod>> GetByHotelAsync(int hotelId, CancellationToken ct = default);
    Task<PayrollPeriod?> GetWithEntriesAsync(int id, CancellationToken ct = default);
}

// ── HR: Training & Performance ──
public interface ITrainingProgramRepository : IRepository<TrainingProgram>
{
    Task<IReadOnlyList<TrainingProgram>> GetByHotelOrGlobalAsync(int? hotelId, CancellationToken ct = default);
}

public interface ITrainingEnrollmentRepository : IRepository<TrainingEnrollment>
{
    Task<IReadOnlyList<TrainingEnrollment>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default);
}

public interface IPerformanceReviewRepository : IRepository<PerformanceReview>
{
    Task<IReadOnlyList<PerformanceReview>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default);
}

// ── HR: Legal & Insurance ──
public interface IEmploymentContractRepository : IRepository<EmploymentContract>
{
    Task<IReadOnlyList<EmploymentContract>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default);
}

public interface IInsuranceRecordRepository : IRepository<InsuranceRecord>
{
    Task<IReadOnlyList<InsuranceRecord>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default);
}
