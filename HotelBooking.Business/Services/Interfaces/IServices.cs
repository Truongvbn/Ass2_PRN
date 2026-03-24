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
    Task<ServiceResult> PayExtraAsync(int bookingId, CancellationToken ct = default);
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

public class AiResponseDto
{
    public string Answer { get; set; } = string.Empty;
    public string? Action { get; set; }
    public string? ActionData { get; set; }
}
