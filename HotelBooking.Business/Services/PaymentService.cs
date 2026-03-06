using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IBookingService _bookingService;
    private readonly IMapper _mapper;

    public PaymentService(IPaymentRepository paymentRepo, IBookingRepository bookingRepo, IBookingService bookingService, IMapper mapper)
    {
        _paymentRepo = paymentRepo;
        _bookingRepo = bookingRepo;
        _bookingService = bookingService;
        _mapper = mapper;
    }

    public async Task<ServiceResult<PaymentDto>> ProcessPaymentAsync(CreatePaymentDto dto, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetByIdAsync(dto.BookingId, ct);
        if (booking is null) return ServiceResult<PaymentDto>.Failure("Booking not found", "NOT_FOUND");
        if (booking.Status == BookingStatus.Cancelled)
            return ServiceResult<PaymentDto>.Failure("Cannot pay for a cancelled booking", "INVALID_STATE");

        // Check if already paid
        var existing = await _paymentRepo.GetByBookingIdAsync(dto.BookingId, ct);
        if (existing is not null && existing.Status == PaymentStatus.Completed)
            return ServiceResult<PaymentDto>.Failure("This booking has already been paid", "ALREADY_PAID");

        if (!Enum.TryParse<PaymentMethod>(dto.Method, out var method))
            return ServiceResult<PaymentDto>.Failure("Invalid payment method", "VALIDATION");

        var payment = new Payment
        {
            BookingId = dto.BookingId,
            Amount = booking.TotalPrice, // Server-side amount, never trust client
            Method = method,
            Status = PaymentStatus.Completed, // Simulated instant payment
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepo.AddAsync(payment, ct);

        // Auto-confirm booking after payment
        await _bookingService.ConfirmBookingAsync(booking.Id, ct);

        return ServiceResult<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment));
    }

    public async Task<ServiceResult<PaymentDto>> GetPaymentByBookingAsync(int bookingId, CancellationToken ct = default)
    {
        var payment = await _paymentRepo.GetByBookingIdAsync(bookingId, ct);
        if (payment is null) return ServiceResult<PaymentDto>.Failure("Payment not found", "NOT_FOUND");
        return ServiceResult<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment));
    }
}
