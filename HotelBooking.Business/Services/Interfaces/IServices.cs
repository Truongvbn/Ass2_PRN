using HotelBooking.Business.DTOs;

namespace HotelBooking.Business.Services.Interfaces;

public interface IRoomService
{
    Task<ServiceResult<IReadOnlyList<RoomListDto>>> SearchRoomsAsync(int? hotelId, int? roomTypeId, decimal? minPrice, decimal? maxPrice, int? minOccupancy, DateTime? checkIn, DateTime? checkOut, CancellationToken ct = default);
    Task<ServiceResult<RoomDto>> GetRoomByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<RoomDto>> CreateRoomAsync(CreateRoomDto dto, CancellationToken ct = default);
    Task<ServiceResult<RoomDto>> UpdateRoomAsync(UpdateRoomDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteRoomAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<RoomTypeDto>> GetRoomTypesAsync(CancellationToken ct = default);
}

public interface IBookingService
{
    Task<ServiceResult<BookingDto>> CreateBookingAsync(CreateBookingDto dto, string userId, CancellationToken ct = default);
    Task<ServiceResult<BookingDto>> GetBookingByIdAsync(int id, string? userId = null, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<BookingDto>>> GetUserBookingsAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<BookingDto>>> GetAllBookingsAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<BookingDto>>> GetBookingsByHotelAsync(int hotelId, CancellationToken ct = default);
    Task<ServiceResult> ApproveBookingAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> RejectBookingAsync(int id, string reason, CancellationToken ct = default);
    Task<ServiceResult> ConfirmBookingAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> CancelBookingAsync(int id, string userId, string? reason = null, CancellationToken ct = default);
    Task<ServiceResult> AdminCancelBookingAsync(int id, string? reason = null, CancellationToken ct = default);
    Task<ServiceResult> CheckInAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> MarkNoShowAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> CompleteBookingAsync(int id, CheckoutBookingDto? checkoutDto = null, CancellationToken ct = default);
}

public interface IHotelService
{
    Task<ServiceResult<IReadOnlyList<HotelDto>>> GetAllHotelsAsync(CancellationToken ct = default);
    Task<ServiceResult<HotelDto>> GetHotelByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<HotelDto>>> GetHotelsByStaffAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<HotelDto>> CreateHotelAsync(CreateHotelDto dto, CancellationToken ct = default);
    Task<ServiceResult<HotelDto>> UpdateHotelAsync(UpdateHotelDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteHotelAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<HotelStaffDto>>> GetHotelStaffAsync(int hotelId, CancellationToken ct = default);
    Task<ServiceResult> AssignStaffAsync(AssignStaffDto dto, CancellationToken ct = default);
    Task<ServiceResult> RemoveStaffAsync(int hotelId, string userId, CancellationToken ct = default);
}

public interface IPaymentService
{
    Task<ServiceResult<PaymentDto>> ProcessPaymentAsync(CreatePaymentDto dto, CancellationToken ct = default);
    Task<ServiceResult<PaymentDto>> GetPaymentByBookingAsync(int bookingId, CancellationToken ct = default);
    Task<ServiceResult> RefundAsync(int bookingId, decimal? amount = null, string? reason = null, CancellationToken ct = default);
}

public interface IReviewService
{
    Task<ServiceResult<ReviewDto>> CreateReviewAsync(CreateReviewDto dto, string userId, CancellationToken ct = default);
    Task<ServiceResult<ReviewDto>> UpdateReviewAsync(UpdateReviewDto dto, string userId, CancellationToken ct = default);
    Task<ServiceResult> DeleteReviewAsync(int id, string userId, bool isAdmin, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<ReviewDto>>> GetRoomReviewsAsync(int roomId, CancellationToken ct = default);
    Task<ServiceResult<ReviewCommentDto>> AddCommentAsync(CreateReviewCommentDto dto, string userId, CancellationToken ct = default);
    Task<ServiceResult> DeleteCommentAsync(int id, string userId, bool isAdmin, CancellationToken ct = default);
}

public interface ITicketService
{
    Task<ServiceResult<TicketDto>> CreateTicketAsync(CreateTicketDto dto, string userId, CancellationToken ct = default);
    Task<ServiceResult<TicketDto>> GetTicketByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<TicketDto>>> GetUserTicketsAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<TicketDto>>> GetActiveTicketsAsync(CancellationToken ct = default);
    Task<ServiceResult> AssignTicketAsync(int ticketId, string staffId, CancellationToken ct = default);
    Task<ServiceResult> UpdateTicketStatusAsync(int ticketId, string newStatus, string userId, bool isStaff, CancellationToken ct = default);
}

public interface IAiAssistantService
{
    Task<ServiceResult<IReadOnlyList<RoomListDto>>> RecommendRoomsAsync(RoomPreferenceDto preferences, CancellationToken ct = default);
    Task<ServiceResult<AiResponseDto>> AnswerQuestionAsync(string question, int? roomId, CancellationToken ct = default);
}

// ── HR Services ──
public interface IEmployeeService
{
    Task<ServiceResult<IReadOnlyList<EmployeeListItemDto>>> GetEmployeesByHotelAsync(int hotelId, CancellationToken ct = default);
    Task<ServiceResult<EmployeeDto>> GetEmployeeByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken ct = default);
    Task<ServiceResult<EmployeeDto>> UpdateEmployeeAsync(UpdateEmployeeDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeactivateEmployeeAsync(int id, CancellationToken ct = default);
}

public interface IShiftService
{
    Task<ServiceResult<IReadOnlyList<WorkShiftDto>>> GetShiftsByHotelAsync(int hotelId, CancellationToken ct = default);
    Task<ServiceResult<WorkShiftDto>> CreateShiftAsync(CreateWorkShiftDto dto, CancellationToken ct = default);
    Task<ServiceResult<WorkShiftDto>> UpdateShiftAsync(UpdateWorkShiftDto dto, CancellationToken ct = default);
    Task<ServiceResult<WorkShiftDto>> GetShiftByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> ToggleShiftActiveAsync(int id, bool isActive, CancellationToken ct = default);

    Task<ServiceResult<IReadOnlyList<ShiftAssignmentDto>>> GetScheduleByHotelAsync(int hotelId, DateTime start, DateTime end, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<ShiftAssignmentDto>>> GetScheduleByEmployeeAsync(int employeeId, DateTime start, DateTime end, CancellationToken ct = default);
    Task<ServiceResult<ShiftAssignmentDto>> AssignShiftAsync(CreateShiftAssignmentDto dto, CancellationToken ct = default);
}

public interface IAttendanceService
{
    Task<ServiceResult<AttendanceDto>> RecordAttendanceAsync(RecordAttendanceDto dto, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<AttendanceDto>>> GetAttendanceByHotelAsync(int hotelId, DateTime start, DateTime end, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<AttendanceDto>>> GetAttendanceByEmployeeAsync(int employeeId, DateTime start, DateTime end, CancellationToken ct = default);
}

public interface IPayrollService
{
    Task<ServiceResult<IReadOnlyList<PayrollPeriodDto>>> GetPayrollPeriodsByHotelAsync(int hotelId, CancellationToken ct = default);
    Task<ServiceResult<PayrollPeriodDto>> CreatePayrollPeriodAsync(CreatePayrollPeriodDto dto, CancellationToken ct = default);
    Task<ServiceResult> CalculatePayrollAsync(int payrollPeriodId, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<PayrollEntryDto>>> GetPayrollEntriesAsync(int payrollPeriodId, CancellationToken ct = default);
    Task<ServiceResult> ApprovePayrollAsync(int payrollPeriodId, CancellationToken ct = default);
    Task<ServiceResult> MarkPayrollPaidAsync(int payrollPeriodId, CancellationToken ct = default);
}

public interface ITrainingService
{
    Task<ServiceResult<IReadOnlyList<TrainingProgramDto>>> GetTrainingProgramsAsync(int? hotelId, CancellationToken ct = default);
    Task<ServiceResult<TrainingProgramDto>> CreateTrainingProgramAsync(CreateTrainingProgramDto dto, CancellationToken ct = default);
    Task<ServiceResult<TrainingProgramDto>> GetTrainingProgramByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<TrainingProgramDto>> UpdateTrainingProgramAsync(UpdateTrainingProgramDto dto, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<TrainingEnrollmentDto>>> GetEnrollmentsByEmployeeAsync(int employeeId, CancellationToken ct = default);
    Task<ServiceResult<TrainingEnrollmentDto>> EnrollEmployeeAsync(EnrollTrainingDto dto, CancellationToken ct = default);
}

public interface IPerformanceService
{
    Task<ServiceResult<IReadOnlyList<PerformanceReviewDto>>> GetReviewsByEmployeeAsync(int employeeId, CancellationToken ct = default);
    Task<ServiceResult<PerformanceReviewDto>> CreateReviewAsync(CreatePerformanceReviewDto dto, string reviewerId, CancellationToken ct = default);
}

public interface ILegalComplianceService
{
    Task<ServiceResult<IReadOnlyList<EmploymentContractDto>>> GetContractsByEmployeeAsync(int employeeId, CancellationToken ct = default);
    Task<ServiceResult<EmploymentContractDto>> CreateContractAsync(CreateEmploymentContractDto dto, CancellationToken ct = default);

    Task<ServiceResult<IReadOnlyList<InsuranceRecordDto>>> GetInsuranceByEmployeeAsync(int employeeId, CancellationToken ct = default);
    Task<ServiceResult<InsuranceRecordDto>> CreateInsuranceRecordAsync(CreateInsuranceRecordDto dto, CancellationToken ct = default);
}

public class AiResponseDto
{
    public string Answer { get; set; } = string.Empty;
    public string? Action { get; set; }
    public string? ActionData { get; set; }
}
